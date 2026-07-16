IMAGE_NAME="securities-masterdata"

echo "Building $IMAGE_NAME..."
docker build -f Dockerfile --platform linux/arm64 -t $IMAGE_NAME:latest ..

echo "Tagging $IMAGE_NAME..."
docker tag $IMAGE_NAME:latest 192.168.68.63:5000/$IMAGE_NAME:latest

echo "Pushing $IMAGE_NAME..."
docker push 192.168.68.63:5000/$IMAGE_NAME:latest

echo "Done."