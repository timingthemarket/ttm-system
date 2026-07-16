IMAGE_NAME="riksbanken-raw"

echo "Building $IMAGE_NAME..."
docker build -f Dockerfile -t $IMAGE_NAME:latest ..

echo "Tagging $IMAGE_NAME..."
docker tag $IMAGE_NAME:latest 192.168.68.63:5000/$IMAGE_NAME:latest

echo "Pushing $IMAGE_NAME..."
docker push 192.168.68.63:5000/$IMAGE_NAME:latest

echo "Done."