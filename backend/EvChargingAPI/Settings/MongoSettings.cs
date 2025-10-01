/* 
 * File: MongoSettings.cs
 * Author: <YourName>
 * Description: Settings mapping for MongoDB connection (populated from appsettings.json)
 */

namespace EvChargingAPI.Settings
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}
