﻿// Application Service Plan (Server Farm)
resource /*${1:appServicePlan}*/appServicePlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: /*${2:'name'}*/'name'
  location: /*${3:location}*/'location'
  sku: {
    name: /*${4:'name'}*/'skuname'
    capacity: /*${5:capacity}*/4
  }
}
