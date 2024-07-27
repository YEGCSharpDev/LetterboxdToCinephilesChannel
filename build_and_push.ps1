# build_and_push.ps1

# Set your Docker Hub username and repository name
$DOCKER_USERNAME = "preface8675"
$REPO_NAME = "cinephilechannelpush"
$VERSION = "v1.0.0"  # Change this for each new version

# Full image name
$FULL_IMAGE_NAME = "${DOCKER_USERNAME}/${REPO_NAME}"

Write-Host "Building Docker image: $FULL_IMAGE_NAME"

# Build the Docker image
docker build -t ${FULL_IMAGE_NAME}:latest .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker build failed. Exiting."
    exit 1
}

Write-Host "Tagging image with version: $VERSION"

# Tag with version
docker tag ${FULL_IMAGE_NAME}:latest ${FULL_IMAGE_NAME}:$VERSION

Write-Host "Pushing images to Docker Hub"

# Push to Docker Hub
docker push ${FULL_IMAGE_NAME}:latest
docker push ${FULL_IMAGE_NAME}:$VERSION

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to push images to Docker Hub. Make sure you're logged in and have the correct permissions."
    exit 1
}

Write-Host "Image built and pushed successfully!"
Write-Host "Latest: ${FULL_IMAGE_NAME}:latest"
Write-Host "Versioned: ${FULL_IMAGE_NAME}:$VERSION"
