#!/bin/bash
# Deploy C0BR4 v3.4 Classical to production container

set -e

echo "=== C0BR4 v3.4 Classical Deployment ==="

# Stop current container
echo "Stopping current container..."
docker stop c0br4-production-v31 || true

# Extract deployment files
echo "Extracting deployment archives..."
cd /home/patss
tar -xzf c0br4_v3.4_classical_deployment.tar.gz
tar -xzf c0br4_config.tar.gz

# Backup current engine
echo "Backing up current engine..."
docker exec c0br4-production-v31 cp -r /lichess-bot/engines /lichess-bot/engines.backup || true

# Copy new engine to container
echo "Copying new engine binary..."
docker cp engines/C0BR4_v3.4_classical_20241221 c0br4-production-v31:/lichess-bot/engines/

# Copy new config to container
echo "Copying new config..."
docker cp config.yml c0br4-production-v31:/lichess-bot/config.yml

# Start container
echo "Starting container..."
docker start c0br4-production-v31

# Verify container is running
echo "Waiting for container to start..."
sleep 5
docker ps | grep c0br4-production-v31

echo ""
echo "=== Deployment Complete ==="
echo "Checking logs..."
docker logs c0br4-production-v31 --tail=20
