#!/bin/bash

# Build script for portfolio-explorer Docker image and run docker-compose
# This script builds the Dockerfile.Export image to avoid rebuilding it multiple times

set -e

IMAGE_NAME="portfolio-explorer"
IMAGE_TAG="latest"

echo "Building Docker image: ${IMAGE_NAME}:${IMAGE_TAG}"
echo "Using Dockerfile: Dockerfile.Export"

sudo docker build --tag "${IMAGE_NAME}:${IMAGE_TAG}" -f Dockerfile.Export .

echo "Successfully built ${IMAGE_NAME}:${IMAGE_TAG}"
echo "Starting docker-compose services..."

sudo docker compose up -d --build