### 1

```
docker build --tag 'securities-masterdata' .
```

### 2

Run docker compose in detatched mode and compatability ro enable memory and cpu restrictions

```
docker run --detach 'securities-masterdata'
```

## Important information

When adding a new *event*-contract it needs to be added in the `/Events` folder in the `.Shared` project. 
The mapping of events in MassTransit is made by the nameSpace and classname. So for example the namespace `ttm_system.Shared.Events.Infra` and classname 
`SystemErrorEvent` can only be mapped to events that has the same namesspace + classname.

## Compose

```
sudo docker-compose --compatibility up -d --build
```