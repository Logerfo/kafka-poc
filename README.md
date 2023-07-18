# Kafka with C# POC
## Deploying dependencies
Run the following command:
```sh
docker-compose up -d
```
And it will be deployed:
- Latest Kafka on port 29092
- Latest Zookeeper on port 22181
- Latest Kafdrop on port 8080

## Building
I recommend using VSCode and running the build task configured in `.vscode/tasks.json`, but you can build through the command line as well:
```sh
dotnet build
```

## Running
To see all the available commands, run:
```sh
./Logerfo.Kafka.exe
```
