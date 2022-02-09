// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
import * as path from "path";
import vscode, { Uri } from "vscode";
import { ext } from "../extensionVariables";
import { Command } from "./types";
import { LanguageClient } from "vscode-languageclient/node";
import {
  //AzExtTreeItem,
  IActionContext,
  IAzureQuickPickItem,
  parseError,
} from "vscode-azureextensionui";
import { AzureAccount } from "../azure-account.api";
//import { SubscriptionTreeItem } from "../tree/SubscriptionTreeItem";
import { SubscriptionClient } from "@azure/arm-subscriptions";
import { ResourceManagementClient } from "@azure/arm-resources";
import { DefaultAzureCredential } from "@azure/identity";
import {
  ManagementGroupsAPI,
  ManagementGroup,
} from "@azure/arm-managementgroups";
import {
  SubscriptionClientContext,
  Subscriptions,
} from "@azure/arm-subscriptions";
import { appendToOutputChannel } from "../utils/logger";
//import { EmptyTreeItem } from "../tree/EmptyTreeItem";
import { SubscriptionTreeItem } from "../tree/SubscriptionTreeItem";

export class DeployCommand implements Command {
  public readonly id = "bicep.deploy";
  public constructor(private readonly client: LanguageClient) {}

  public async execute(
    _context: IActionContext,
    documentUri?: vscode.Uri | undefined
  ): Promise<void> {
    documentUri ??= vscode.window.activeTextEditor?.document.uri;

    if (!documentUri) {
      return;
    }

    appendToOutputChannel(
      `Started deployment of "${path.basename(documentUri.fsPath)}"`
    );

    if (documentUri.scheme === "output") {
      // The output panel in VS Code was implemented as a text editor by accident. Due to breaking change concerns,
      // it won't be fixed in VS Code, so we need to handle it on our side.
      // See https://github.com/microsoft/vscode/issues/58869#issuecomment-422322972 for details.
      vscode.window.showInformationMessage(
        "We are unable to get the Bicep file to build when the output panel is focused. Please focus a text editor first when running the command."
      );

      return;
    }

    try {
      const subscription =
        await ext.tree.showTreeItemPicker<SubscriptionTreeItem>(
          '',
          _context
        );

      const deploymentScope = await getDeploymentScope(_context);

      const parameterFilePath = await selectParameterFile(
        _context,
        documentUri
      );

      if (deploymentScope == "ResourceGroup") {
        await handleResourceGroupDeployment(
          _context,
          documentUri.fsPath,
          parameterFilePath,
          deploymentScope,
          this.client
        );
      } else if (deploymentScope == "Subscription") {
        await handleSubscriptionDeployment(
          _context,
          documentUri.fsPath,
          parameterFilePath,
          deploymentScope,
          this.client
        );
      } else if (deploymentScope == "ManagementGroup") {
        await handleManagementGroupDeployment(
          _context,
          documentUri.fsPath,
          parameterFilePath,
          deploymentScope,
          this.client
        );
      }
    } catch (err) {
      this.client.error("Deploy failed", parseError(err).message, true);
    }
  }
}

async function handleSubscriptionDeployment(
  context: IActionContext,
  documentPath: string,
  parameterFilePath: string,
  deploymentScope: string,
  client: LanguageClient
) {
  const subscriptions = await loadSubscriptionItems();
  const subscription = await context.ui.showQuickPick(subscriptions, {
    placeHolder: "Please select subscription",
  });
  const subscriptionId = subscription?.subscription.subscriptionId;

  if (subscriptionId) {
  const locations = await loadLocationItems(subscriptionId);
  const location = await context.ui.showQuickPick(locations, {
    placeHolder: "Please select location",
  });

    if (location) {
      const deployOutput: string = await client.sendRequest(
        "workspace/executeCommand",
        {
          command: "deploy",
          arguments: [
            documentPath,
            parameterFilePath,
            subscription.subscription.id,
            deploymentScope,
            location.label,
          ],
        }
      );
      appendToOutputChannel(deployOutput);
    }
  }
}

async function loadManagementGroupItems() {
  const managementGroupsAPI = new ManagementGroupsAPI(
    new DefaultAzureCredential()
  );
  const managementGroups = await managementGroupsAPI.managementGroups.list();
  const managementGroupsArray: ManagementGroup[] = [];

  for await (const managementGroup of managementGroups) {
    managementGroupsArray.push(managementGroup);
  }

  managementGroupsArray.sort((a, b) =>
    (a.name || "").localeCompare(b.name || "")
  );
  return managementGroupsArray.map((mg) => ({
    label: mg.name || "",
    mg,
  }));
}

async function loadLocationItems(subscriptionId: string) {
  const azureAccount = vscode.extensions.getExtension<AzureAccount>(
    "ms-vscode.azure-account"
  )!.exports;
  const session = azureAccount.sessions[0];
  const subscriptionClientContext = new SubscriptionClientContext(
    session.credentials2
  );
  const subscription = new Subscriptions(subscriptionClientContext);
  const locations = await subscription.listLocations(subscriptionId);

  locations.sort((a, b) => (a.name || "").localeCompare(b.name || ""));
  return locations.map((location) => ({
    label: location.name || "",
    location,
  }));
}

async function handleManagementGroupDeployment(
  context: IActionContext,
  documentPath: string,
  parameterFilePath: string,
  deploymentScope: string,
  client: LanguageClient
) {
  const managementGroupItems = loadManagementGroupItems();
  const managementGroup = await context.ui.showQuickPick(managementGroupItems, {
    placeHolder: "Please select management group",
  });

  const location = await vscode.window.showInputBox({
    placeHolder: "Please enter location",
  });

  const managementGroupId = managementGroup?.mg.id;
  if (managementGroupId && location) {
    const deployOutput: string = await client.sendRequest(
      "workspace/executeCommand",
      {
        command: "deploy",
        arguments: [
          documentPath,
          parameterFilePath,
          managementGroupId,
          deploymentScope,
          location,
        ],
      }
    );
    appendToOutputChannel(deployOutput);
  }
}

async function handleResourceGroupDeployment(
  context: IActionContext,
  documentPath: string,
  parameterFilePath: string,
  deploymentScope: string,
  client: LanguageClient
) {
  const subscriptions = await loadSubscriptionItems();
  const subscription = await context.ui.showQuickPick(subscriptions, {
    placeHolder: "Please select subscription",
  });
  const subscriptionId = subscription?.subscription.subscriptionId;

  if (subscriptionId) {
    const resourceGroupItems = loadResourceGroupItems(subscriptionId);
    const resourceGroup = await context.ui.showQuickPick(resourceGroupItems, {
      placeHolder: "Please select resource group",
    });

    const resourceGroupId = resourceGroup?.resourceGroup.id;

    if (resourceGroupId) {
      const deployOutput: string = await client.sendRequest(
        "workspace/executeCommand",
        {
          command: "deploy",
          arguments: [
            documentPath,
            parameterFilePath,
            resourceGroupId,
            deploymentScope,
            "",
          ],
        }
      );
      appendToOutputChannel(deployOutput);
    }
  }
}

async function getDeploymentScope(context: IActionContext) {
  const deploymentScopes: IAzureQuickPickItem<string | undefined>[] =
    await createScopesQuickPickList();

  const deploymentScope: IAzureQuickPickItem<string | undefined> =
    await context.ui.showQuickPick(deploymentScopes, {
      canPickMany: false,
      placeHolder: `Select a deployment scope`,
      suppressPersistence: true,
    });

  return deploymentScope.label;
}

async function loadResourceGroupItems(subscriptionId: string) {
  const azureAccount = vscode.extensions.getExtension<AzureAccount>(
    "ms-vscode.azure-account"
  )!.exports;
  const session = azureAccount.sessions[0];

  const resources = new ResourceManagementClient(
    session.credentials2,
    subscriptionId
  );
  const resourceGroups = await listAll(
    resources.resourceGroups,
    resources.resourceGroups.list()
  );
  resourceGroups.sort((a, b) => (a.name || "").localeCompare(b.name || ""));
  return resourceGroups.map((resourceGroup) => ({
    label: resourceGroup.name || "",
    description: resourceGroup.location,
    resourceGroup,
  }));
}

async function loadSubscriptionItems() {
  const azureAccount = vscode.extensions.getExtension<AzureAccount>(
    "ms-vscode.azure-account"
  )!.exports;
  const session = azureAccount.sessions[0];

  const subscriptionClient = new SubscriptionClient(
    session.credentials2,
  );
  const subscriptions = await listAll(
    subscriptionClient.subscriptions,
    subscriptionClient.subscriptions.list()
  );
  subscriptions.sort((a, b) => (a.displayName || "").localeCompare(b.displayName || ""));
  return subscriptions.map((subscription) => ({
    label: subscription.displayName || "",
    subscription,
  }));
}

async function listAll<T>(
  client: { listNext(nextPageLink: string): Promise<PartialList<T>> },
  first: Promise<PartialList<T>>
): Promise<T[]> {
  const all: T[] = [];
  for (
    let list = await first;
    list.length || list.nextLink;
    list = list.nextLink ? await client.listNext(list.nextLink) : []
  ) {
    all.push(...list);
  }
  return all;
}

export interface PartialList<T> extends Array<T> {
  nextLink?: string;
}

export async function selectParameterFile(
  _context: IActionContext,
  sourceUri: Uri | undefined
): Promise<string> {
  const quickPickList: IQuickPickList =
    await createParameterFileQuickPickList();
  const result: IAzureQuickPickItem<IPossibleParameterFile | undefined> =
    await _context.ui.showQuickPick(quickPickList.items, {
      canPickMany: false,
      placeHolder: `Select a parameter file`,
      suppressPersistence: true,
    });

  if (result === quickPickList.browse) {
    const paramsPaths: Uri[] | undefined = await vscode.window.showOpenDialog({
      canSelectMany: false,
      defaultUri: sourceUri,
      openLabel: "Select Parameter File",
    });
    if (paramsPaths && paramsPaths.length == 1) {
      const parameterFilePath = paramsPaths[0].fsPath;
      appendToOutputChannel(
        `Parameter file used in deployment: "${path.basename(
          parameterFilePath
        )}"`
      );
      return parameterFilePath;
    }
  }

  appendToOutputChannel(`Parameter file was not provided`);

  return "";
}

async function createParameterFileQuickPickList(): Promise<IQuickPickList> {
  const none: IAzureQuickPickItem<IPossibleParameterFile | undefined> = {
    label: "$(circle-slash) None",
    data: undefined,
  };
  const browse: IAzureQuickPickItem<IPossibleParameterFile | undefined> = {
    label: "$(file-directory) Browse...",
    data: undefined,
  };

  const pickItems: IAzureQuickPickItem<IPossibleParameterFile | undefined>[] = [
    none,
  ].concat([browse]);

  return {
    items: pickItems,
    none,
    browse,
  };
}

async function createScopesQuickPickList(): Promise<
  IAzureQuickPickItem<string | undefined>[]
> {
  const managementGroup: IAzureQuickPickItem<string | undefined> = {
    label: "ManagementGroup",
    data: undefined,
  };
  const resourceGroup: IAzureQuickPickItem<string | undefined> = {
    label: "ResourceGroup",
    data: undefined,
  };
  const subscription: IAzureQuickPickItem<string | undefined> = {
    label: "Subscription",
    data: undefined,
  };

  const scopes: IAzureQuickPickItem<string | undefined>[] = [managementGroup]
    .concat([resourceGroup])
    .concat([subscription]);

  return scopes;
}

interface IQuickPickList {
  items: IAzureQuickPickItem<IPossibleParameterFile | undefined>[];
  none: IAzureQuickPickItem<IPossibleParameterFile | undefined>;
  browse: IAzureQuickPickItem<IPossibleParameterFile | undefined>;
}

interface IPossibleParameterFile {
  uri: Uri;
  friendlyPath: string;
  isCloseNameMatch: boolean;
  fileNotFound?: boolean;
}
