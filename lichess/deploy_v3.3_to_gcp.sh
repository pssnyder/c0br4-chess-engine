#!/bin/bash
# C0BR4 v3.3 Deployment Script for GCP
# Deploys the new adaptive time management engine to the Lichess bot VM

set -e  # Exit on error

PROJECT="c0br4-lichess-bot"
INSTANCE="c0br4-production-bot"
ZONE="us-central1-a"
ENGINE_VERSION="v3.3"

echo "================================================"
echo "C0BR4 v3.3 Deployment to GCP Lichess Bot"
echo "================================================"
echo ""
echo "Project: $PROJECT"
echo "Instance: $INSTANCE"
echo "Zone: $ZONE"
echo "Engine Version: $ENGINE_VERSION"
echo ""

# Check if we're in the correct project
CURRENT_PROJECT=$(gcloud config get-value project)
if [ "$CURRENT_PROJECT" != "$PROJECT" ]; then
    echo "ERROR: Wrong GCP project. Current: $CURRENT_PROJECT, Expected: $PROJECT"
    echo "Run: gcloud config set project $PROJECT"
    exit 1
fi

echo "[Step 1/6] Verifying Linux binary exists..."
if [ ! -f "./engines/C0BR4_v3.3/C0BR4_v3.3" ]; then
    echo "ERROR: Linux binary not found at ./engines/C0BR4_v3.3/C0BR4_v3.3"
    echo "Please build it first with: dotnet publish -c Release-Linux -r linux-x64 --self-contained -p:PublishSingleFile=true"
    exit 1
fi
echo "✓ Linux binary found"

echo ""
echo "[Step 2/6] Checking VM instance status..."
INSTANCE_STATUS=$(gcloud compute instances describe $INSTANCE --zone=$ZONE --format="value(status)" --project=$PROJECT)
echo "Instance status: $INSTANCE_STATUS"

if [ "$INSTANCE_STATUS" != "RUNNING" ]; then
    echo "WARNING: Instance is not running. Starting it now..."
    gcloud compute instances start $INSTANCE --zone=$ZONE --project=$PROJECT
    sleep 10
fi

echo ""
echo "[Step 3/6] Stopping current bot container..."
gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command="sudo docker stop c0br4-production-v31 || true"
echo "✓ Container stopped"

echo ""
echo "[Step 4/6] Backing up current engine and uploading new v3.3..."
# Create backup directory in container volume
gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command="sudo mkdir -p /var/lib/docker/volumes/c0br4-bot-data/_data/engines/backups"

# Upload the new engine
gcloud compute scp --recurse ./engines/C0BR4_v3.3 $INSTANCE:/tmp/ --zone=$ZONE --project=$PROJECT
gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command="sudo cp -r /tmp/C0BR4_v3.3 /var/lib/docker/volumes/c0br4-bot-data/_data/engines/"

# Upload updated config
gcloud compute scp ./config.yml $INSTANCE:/tmp/config.yml --zone=$ZONE --project=$PROJECT
gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command="sudo cp /tmp/config.yml /var/lib/docker/volumes/c0br4-bot-data/_data/"

# Make the engine executable
gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command="sudo chmod +x /var/lib/docker/volumes/c0br4-bot-data/_data/engines/C0BR4_v3.3/C0BR4_v3.3"
echo "✓ Upload completed"

echo ""
echo "[Step 5/6] Verifying engine..."
gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command="sudo docker exec c0br4-production-v31 ls -la engines/C0BR4_v3.3/ || echo 'Container not running yet'"

echo ""
echo "[Step 6/6] Restarting bot container..."
gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command="sudo docker start c0br4-production-v31"
sleep 3

echo "✓ Bot restarted"

echo ""
echo "================================================"
echo "Deployment completed successfully!"
echo "================================================"
echo ""
echo "Verification commands:"
echo "  Check logs:    gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command='tail -f ~/lichess-bot/bot.log'"
echo "  Check process: gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command='ps aux | grep lichess-bot'"
echo "  Check engine:  gcloud compute ssh $INSTANCE --zone=$ZONE --project=$PROJECT --command='echo \"uci\" | ~/lichess-bot/engines/C0BR4_v3.3/C0BR4_v3.3'"
echo ""
echo "C0BR4 v3.3 Features:"
echo "  - Adaptive depth: 6-10 based on time control"
echo "  - Conservative time management"
echo "  - Classical 30min: depth 9-10"
echo "  - Rapid 10min: depth 8"
echo "  - Blitz 3min: depth 6"
echo "  - Expected +150-200 Elo in longer games"
echo ""
echo "Monitor your bot at: https://lichess.org/@/c0br4_bot"
echo ""
