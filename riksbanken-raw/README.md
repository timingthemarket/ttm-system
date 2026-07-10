## 1

```
docker build --tag 'riksbanken-raw' .
```

## 2

```
docker run --detach 'riksbanken-raw' --env-file ./.env
```


## Compose

```
sudo docker-compose --compatibility up -d --build
```