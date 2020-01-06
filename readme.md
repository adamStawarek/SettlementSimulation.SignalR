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
in further simulation).  
These heights are comuted from heightmap so we have to provide
path to it in BitmapDto model i.e:  
```csharp
//in Program.cs
 var heightMap = new BitmapDto()
            {
                Path = @"C:\heightmap.png"
            };


  static void GetTerrains(BitmapDto model)
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
                proxy.Invoke("GetTerrains", model);
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
Response at 06.01.2020 20:00:06
CurrentEpoch: 2
MainRoad: Start: (X: 0, Y: 744), End: (X: 1024, Y: 647)
CurrentGeneration: 305
Buildings: 1532
Roads: 106

LastGeneratedRoads:

Type: Unpaved
Locations:
         (X: 393, Y: 438)
         (X: 394, Y: 438)
         (X: 395, Y: 438)
         (X: 396, Y: 438)
         (X: 397, Y: 438)
         (X: 398, Y: 438)
         (X: 399, Y: 438)
         (X: 400, Y: 438)
         (X: 401, Y: 438)
         (X: 402, Y: 438)
         (X: 403, Y: 438)
         (X: 404, Y: 438)
         (X: 405, Y: 438)
         (X: 406, Y: 438)

LastGeneratedBuildings:
Type: Residence Location: (X: 393, Y: 437)
Type: Residence Location: (X: 395, Y: 439)
Type: Residence Location: (X: 396, Y: 439)
Type: Residence Location: (X: 397, Y: 437)
Type: Residence Location: (X: 398, Y: 437)
Type: Residence Location: (X: 399, Y: 437)
Type: Market Location: (X: 404, Y: 439)
```
