// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
import { workspace, window, Uri } from "vscode";
import { Command } from "./types";
import { LanguageClient } from "vscode-languageclient/node";
import {
  IActionContext,
  IAzureQuickPickItem,
  UserCancelledError,
} from "vscode-azureextensionui";
import path from "path";
import { accessSync, constants } from "fs";
import * as os from "os";

const bicepConfig = "bicep.config";

export class CreateBicepConfiguration implements Command {
  public readonly id = "bicep.createBicepConfiguration";

  public constructor(private readonly client: LanguageClient) {}

  public async execute(
    context: IActionContext,
    documentUri?: Uri | undefined
  ): Promise<void> {
    documentUri ??= window.activeTextEditor?.document.uri; //asdfg refactor
    if (documentUri?.scheme === "output") {
      // The output panel in VS Code was implemented as a text editor by accident. Due to breaking change concerns,
      // it won't be fixed in VS Code, so we need to handle it on our side.
      // See https://github.com/microsoft/vscode/issues/58869#issuecomment-422322972 for details.
      window.showInformationMessage(
        "We are unable to get the Bicep file to build when the output panel is focused. Please focus a text editor first when running the command."
      );
      //asdfg refactor
      return;
    }

    //asdfg
    // eslint-disable-next-line no-debugger
    debugger;

    const currentFolder: string | undefined =
      (documentUri
        ? workspace.getWorkspaceFolder(documentUri)?.uri.fsPath
        : undefined) ?? (workspace.workspaceFolders ?? [])[0].uri.fsPath;

    const picks: IAzureQuickPickItem<string>[] = [];
    let folder = currentFolder;
    try {
      // eslint-disable-next-line no-constant-condition
      while (true) {
        // Will throw if can't access to read/write
        accessSync(folder, constants.W_OK | constants.R_OK);
        picks.push({
          label: folder,
          data: folder,
        });
        const parent = path.dirname(folder);
        if (parent === folder) {
          break;
        }
        folder = parent;
      }
    } catch (err) {
      // Break out of loop
    }

    const browse: IAzureQuickPickItem<string> = {
      label: "Browse...",
      data: "",
    };
    picks.push(browse);

    let configPath: string | undefined;
    const selection = await context.ui.showQuickPick(picks, {
      placeHolder: "Where would you like to save the Bicep configuration file?", //asdfg
    });
    if (selection === browse) {
      let newPath: string = path.join(
        currentFolder ?? os.homedir(),
        bicepConfig
      );
      while (!configPath) {
        const response = await window.showSaveDialog({
          defaultUri: Uri.file(newPath),
          filters: { "Bicep configuration files": [bicepConfig] },
          title: "Where would you like to save the Bicep configuration file?", //asdfg
          saveLabel: "Create",
        });
        if (!response || !response.fsPath) {
          throw new UserCancelledError("browse");
        }

        newPath = response.fsPath;

        if (path.basename(newPath) !== bicepConfig) {
          window.showErrorMessage(
            `A Bicep configuration file must be named ${bicepConfig}`
          );
          newPath = path.join(path.dirname(newPath), bicepConfig);
        } else {
          configPath = newPath;
        }
      }
    } else {
      configPath = path.join(selection.data, bicepConfig);
    }
    if (!configPath) {
      throw new UserCancelledError("browse");
    }

    console.warn(configPath); //asdfg
    await this.client.sendRequest("workspace/executeCommand", {
      command: "editConfiguration",
      arguments: [Uri.file(configPath)],
    });
  }
}
