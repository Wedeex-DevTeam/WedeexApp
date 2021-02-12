
 **Wedeex App**
 
<img src="https://i.imgur.com/k5QfAmn.png" width="500px" />

*WedeexApp enables you to adapt and optimize your energy consumption according to your country energy mix production!*
 - *get a real-time simple indicator about the CO2 emission level linked
   to electrical power generation in France*
  - *follow your microsoft
   surface device consumption in real time (and soon splitted by more consuming application : )*
- *act with efficiency by following our tips according the current state of the power generation*

*Become a master in understanding how electricity production works and how consumption emits CO2.
Act collectively to reduce your electrical devices usage carbon footprint!*

**This repos is an example of Wedeex's API implementation,through a UWP application**

## Installation

Simply compile the solution, according to your architecture, and run the main project "Package"
![Select the "Package" project as startup](https://i.imgur.com/fb0MBDu.png)

## Settings

You can retrieve the real-time indicator by requesting an api-key to subscribe.api@wedeex.com
We offer a freemium limited access to discover our apis.

You will have to configure the [settings file](CSN.Common/Configuration/Configuration.json)

```json
{
  "apiKey": "",
  "consumptionReportUrl": "https://wedeex-api-center.azure-api.net/internal/consumption/report",
  "consumptionSummaryUrl": "https://wedeex-api-center.azure-api.net/consumption/summary",
  "notificationHubConnectionString": "", 
  "notificationHubPath": "",
  "signatureId": "8d5e9b58-7f5d-4064-a664-b91851d06a50",
  "signatureKey": "06z23z/PDpoaWibwqRJUj3siOyzrhBOP5x4bp1AyEXQ=",
  "telemetryInstrumentationKey": ""
}
```

where

```code
"apiKey" is required to fetch Wedeex Realtime indicator's api
"notificationHubConnectionString" is your Azure Notification hub connection string
"notificationHubPath" is your Azure Notification hub path
"signatureId" is required for integrity checks
"signatureKey" is required for integrity checks
"telemetryInstrumentationKey" is your Azure ApplicationInsight telemetry key
```

## Usage

Freemium API Key will let you make 1 call per 6 hour.
You can upgrade it with customs plan using the mail contact above

## Limitations

Wedeex API can provide energy mix production for the French territory. 
More countries will be added in the next releases ;)

Today srumutil.exe is missing in the project. We expect a new version from Windows dev team in a few weeks.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
Please make sure to update tests as appropriate.

## License

[MIT](https://choosealicense.com/licenses/mit/)
