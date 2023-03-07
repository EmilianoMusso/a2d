# a2d - appsettings.json to Docker .env format

This small tool is useful to quickly translate dotnet appsettings.json files to Docker environment format

## Sample

Input file sample

```json
{
  "ConnectionStrings": {
    "MyConnection": "Data Source=localhost;Initial Catalog=Demo;Trusted_Connection=True;"
  },
  "SqlServer": {
    "MaxRetryCount": 30,
    "MaxRetryDelay": "00:05:00",
    "ErrorNumbersToAdd": [] 
  }
}
```

Output sample

```txt
ConnectionStrings__MyConnection=Data Source=localhost;Initial Catalog=Demo;Trusted_Connection=True;
SqlServer__MaxRetryCount=30
SqlServer__MaxRetryDelay=00:05:00
```

## Usage

```bat
a2d.exe --appsettings <PATH_TO_YOUR_APPSETTINGS_FILE>
```

You can obviously redirect the output to a file like this

```bat
a2d.exe --appsettings <PATH_TO_YOUR_APPSETTINGS_FILE> > .env
```