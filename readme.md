# SettlementSimulation.Client
## Get all supported buildings (getting roads is analogical):
Function to get types of buildings
that can be generated.
```csharp
//in Program.cs
 static void GetSupportedBuildings()
        {
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            var conn = new HubConnection(url);
            var proxy = conn.CreateHubProxy("notificationHub");

            try
            {
                conn.Start().Wait();
                proxy.On<string[]>("OnGetSupportedBuildingsResponse", response =>
                    {
                        Console.WriteLine("\nSupported buildings:");
                        response.ToList().ForEach(Console.WriteLine);
                        //Do something
                    });
                proxy.Invoke("GetSupportedBuildings");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
```
Example ouput:
```
Supported buildings:
Court
University
Administration
Church
School
Market
Residence
Tavern

Supported roads:
Unpaved
Paved
```
## Get terrains
Function to get terrain types along with 
their upper height bound( ie lowland with 
UpperHeightBound = 170, and Sand with UpperHeightBound = 145, 
means that all pixels from heightmap 
with intenisities between 146 and 170 will be treated as Lowland 
in further simulation)  
Remark: only pixels that are classyfied as lowland will  be included
 in settlement area (to avoid setting min/max height)
```csharp
//in Program.cs
  static void GetTerrains()
        {
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            var conn = new HubConnection(url);
            var proxy = conn.CreateHubProxy("notificationHub");

            try
            {
                conn.Start().Wait();
                proxy.On<TerrainDto[]>("OnGetTerrainsResponse", response =>
                {
                    Console.WriteLine("\nAll terrains:");
                    response.ToList().ForEach(Console.WriteLine);
                    //do something
                });
                proxy.Invoke("GetTerrains");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
```
Example ouput:
```
All terrains:
Type: DeepWater UpperHeightBound: 75
Type: Water UpperHeightBound: 130
Type: Sand UpperHeightBound: 145
Type: Lowland UpperHeightBound: 170
Type: HighGround UpperHeightBound: 200
Type: MountainBottom UpperHeightBound: 230
Type: MountainTop UpperHeightBound: 255
```
## Run simulation
```csharp
  static void RunSimulation(RunSimulationRequest request)
        {
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            var conn = new HubConnection(url);
            var proxy = conn.CreateHubProxy("notificationHub");

            try
            {
                conn.Start().Wait();
                proxy.On<RunSimulationResponse>("OnSettlementStateUpdate", response =>
                {
                    Console.Clear();
                    Console.WriteLine($"Response at {DateTime.UtcNow:G}");
                    Console.WriteLine(response);
                    //do something
                });
                proxy.On<string>("OnFinished", Console.WriteLine);
                proxy.On<string>("OnException", Console.WriteLine);
                proxy.Invoke("RunSimulation", request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
```
LastGeneratedRoads and LastGeneratedBuildings from 
RunSimulationResponse are collection of structures 
created in last generetion. So to follow simulation 
state one may need to set BreakpointStep to 1
when sending RunSimulationRequest.  

Example output:
```
Response at 05.01.2020 23:48:33
CurrentEpoch: 2
CurrentGeneration: 190
Buildings: 1136
Roads: 61

LastGeneratedRoads:
Type: Unpaved 
Locations:
         (X: 363, Y: 352)
         (X: 363, Y: 353)
         (X: 363, Y: 354)
         (X: 363, Y: 355)
         (X: 363, Y: 356)
         (X: 363, Y: 357)
         (X: 363, Y: 358)
         (X: 363, Y: 359)
         (X: 363, Y: 360)
         (X: 363, Y: 361)
         (X: 363, Y: 362)
         (X: 363, Y: 363)
         (X: 363, Y: 364)
         (X: 363, Y: 365)
         (X: 363, Y: 366)
         (X: 363, Y: 367)
         (X: 363, Y: 368)
         (X: 363, Y: 369)
         (X: 363, Y: 370)
         (X: 363, Y: 371)
         (X: 363, Y: 374)
         (X: 363, Y: 375)

LastGeneratedBuildings:
Type: Tavern Location: (X: 364, Y: 353)
Type: Residence Location: (X: 362, Y: 353)
Type: Residence Location: (X: 362, Y: 354)
Type: Residence Location: (X: 362, Y: 355)
Type: Residence Location: (X: 364, Y: 357)
Type: Residence Location: (X: 364, Y: 361)
Type: Residence Location: (X: 362, Y: 363)
Type: Residence Location: (X: 362, Y: 364)
Type: School Location: (X: 364, Y: 367)
Type: Market Location: (X: 362, Y: 369)
Type: Residence Location: (X: 364, Y: 371)
```
