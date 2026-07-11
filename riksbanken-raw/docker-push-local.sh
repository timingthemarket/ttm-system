IMAGE_NAME="riksbanken-raw"

echo "Building $IMAGE_NAME..."
docker build -f Dockerfile --platform linux/arm64 -t $IMAGE_NAME:latest ..

echo "Tagging $IMAGE_NAME..."
docker tag $IMAGE_NAME:latest host.docker.internal:5000/$IMAGE_NAME:latest

echo "Pushing $IMAGE_NAME..."
docker push host.docker.internal:5000/$IMAGE_NAME:latest

echo "Done."