# C0BR4 Chess Engine - Multi-Deployment Guide

Welcome to the C0BR4 chess engine deployment guide! This document covers three deployment scenarios to meet different needs.

## 🎯 **Deployment Options Overview**

| Deployment Type | Use Case | Engine Binary | Performance | Complexity |
|----------------|----------|---------------|-------------|------------|
| **Local Windows** | Development, Testing | Windows .exe | Good | Simple |
| **Local Docker** | Development, Linux Testing | Native Linux C# | Better | Medium |
| **Cloud Docker** | Production, 24/7 Play | Optimized Linux C# | Best | Advanced |

---

## 🖥️ **Option 1: Local Windows (Current Setup)**

**Best for:** Quick development, testing changes, Windows-only environment

### Current Status: ✅ **WORKING**
Your current setup with `C0BR4_v2.9.exe` is fully functional.

### Configuration:
- **Config file:** `config.yml`
- **Engine path:** `./engines/C0BR4_v2.9.exe`
- **Platform:** Windows native

### Usage:
```bash
# Just run as you currently do:
python lichess-bot.py
```

### Advantages:
- ✅ No setup required
- ✅ Fastest to start developing
- ✅ Native Windows performance
- ✅ Easy debugging

### Disadvantages:
- ❌ Windows-only
- ❌ No cloud deployment preparation
- ❌ Potential compatibility issues

---

## 🐳 **Option 2: Local Docker (Recommended for Development)**

**Best for:** Testing Docker setup, Linux compatibility, cloud preparation

### Setup Steps:

#### Step 1: Build Local Docker Image
```bash
# Make script executable (Git Bash on Windows)
chmod +x docker/build-local.sh

# Build the local Docker image
./docker/build-local.sh
```

#### Step 2: Run Locally
```bash
# Set your Lichess token
export LICHESS_TOKEN="your_lichess_bot_token_here"

# Run in foreground (for testing)
docker run --rm -it \
  -e LICHESS_TOKEN=$LICHESS_TOKEN \
  -v "$(pwd)/game_records:/lichess-bot/game_records" \
  --name c0br4-bot-local \
  c0br4-lichess-local:v2.9-local

# Or run in background
docker run -d \
  -e LICHESS_TOKEN=$LICHESS_TOKEN \
  -v "$(pwd)/game_records:/lichess-bot/game_records" \
  --name c0br4-bot-local \
  --restart unless-stopped \
  c0br4-lichess-local:v2.9-local
```

#### Step 3: Monitor and Control
```bash
# View logs
docker logs -f c0br4-bot-local

# Stop container
docker stop c0br4-bot-local

# Remove container
docker rm c0br4-bot-local

# Shell access for debugging
docker exec -it c0br4-bot-local /bin/bash
```

### Configuration:
- **Config file:** `config-docker-local.yml`
- **Engine path:** `./engines/c0br4/C0BR4ChessEngine` (Linux binary)
- **Platform:** Docker container with Linux

### Advantages:
- ✅ Native Linux C# performance (likely faster than .exe)
- ✅ Perfect preparation for cloud deployment
- ✅ Consistent environment across platforms
- ✅ Easy to test and rebuild
- ✅ Isolated from host system

### Disadvantages:
- ❌ Requires Docker setup
- ❌ Slightly more complex than Windows exe
- ❌ Build time for initial setup

---

## ☁️ **Option 3: Cloud Deployment (Production)**

**Best for:** 24/7 operation, tournament play, professional deployment

### Platforms Supported:
1. **Google Cloud Platform (GCP)** - Recommended
2. **Railway.app** - Simple alternative
3. **DigitalOcean** - Cost-effective
4. **Any cloud provider with Docker support**

### GCP Deployment (Recommended):

#### Prerequisites:
```bash
# Install Google Cloud CLI
# Configure authentication
gcloud auth login
gcloud config set project YOUR_PROJECT_ID

# Enable required APIs
gcloud services enable compute.googleapis.com
gcloud services enable containerregistry.googleapis.com
```

#### Step 1: Build and Push Cloud Image
```bash
# Build cloud-optimized image
./docker/build-cloud.sh

# Tag for Google Container Registry
docker tag c0br4-lichess-bot:v2.9-cloud gcr.io/YOUR_PROJECT_ID/c0br4-lichess-bot:v2.9

# Configure Docker for GCR
gcloud auth configure-docker

# Push to registry
docker push gcr.io/YOUR_PROJECT_ID/c0br4-lichess-bot:v2.9
```

#### Step 2: Deploy to Compute Engine
```bash
# Create optimized VM instance
gcloud compute instances create c0br4-lichess-bot \
  --zone=us-central1-a \
  --machine-type=e2-medium \
  --boot-disk-size=20GB \
  --boot-disk-type=pd-standard \
  --image-family=cos-stable \
  --image-project=cos-cloud \
  --container-image=gcr.io/YOUR_PROJECT_ID/c0br4-lichess-bot:v2.9 \
  --container-env=LICHESS_TOKEN=your_bot_token \
  --container-restart-policy=always \
  --tags=http-server,https-server \
  --metadata=google-logging-enabled=true
```

#### Step 3: Monitor and Maintain
```bash
# Check VM status
gcloud compute instances describe c0br4-lichess-bot --zone=us-central1-a

# View container logs
gcloud compute ssh c0br4-lichess-bot --zone=us-central1-a \
  --command="docker logs \$(docker ps -q) --tail=100"

# Update deployment
gcloud compute instances update-container c0br4-lichess-bot \
  --zone=us-central1-a \
  --container-image=gcr.io/YOUR_PROJECT_ID/c0br4-lichess-bot:v2.9-updated
```

### Configuration:
- **Config file:** `config-docker-cloud.yml`
- **Engine path:** `./engines/c0br4/C0BR4ChessEngine` (Optimized Linux binary)
- **Platform:** GCP Compute Engine

### Cost Estimates:
- **e2-medium instance**: ~$25-30/month (24/7)
- **Network/Storage**: ~$2-3/month
- **Total**: ~$27-33/month

### Advantages:
- ✅ 24/7 operation
- ✅ Professional-grade deployment
- ✅ Optimal performance (native Linux C#)
- ✅ Scalable and maintainable
- ✅ Automatic restarts and monitoring
- ✅ Industry-standard approach

### Disadvantages:
- ❌ Monthly cost
- ❌ Requires cloud platform knowledge
- ❌ More complex setup and maintenance

---

## 🚀 **Recommended Development Workflow**

### Phase 1: Local Development
1. **Start with Windows exe** for quick iteration and testing
2. **Switch to Local Docker** when ready to test cloud compatibility
3. **Compare performance** between Windows exe and Linux Docker

### Phase 2: Cloud Preparation  
1. **Test Local Docker thoroughly** - this is identical to cloud environment
2. **Verify engine performance** and game record storage
3. **Test with different time controls** and opponent types

### Phase 3: Cloud Deployment
1. **Deploy to GCP** using the provided scripts
2. **Monitor initial performance** and resource usage
3. **Optimize settings** based on actual cloud performance

---

## 🔧 **Configuration Files Summary**

| File | Purpose | Engine Binary |
|------|---------|---------------|
| `config.yml` | Local Windows development | `C0BR4_v2.9.exe` |
| `config-docker-local.yml` | Local Docker testing | `C0BR4ChessEngine` (Linux) |
| `config-docker-cloud.yml` | Cloud production | `C0BR4ChessEngine` (Optimized) |

---

## 🎯 **Next Steps**

1. **✅ Keep your current Windows setup working** - it's perfect for development
2. **🧪 Try Local Docker** - run `./docker/build-local.sh` to test
3. **📊 Compare performance** between Windows exe and Linux Docker
4. **☁️ Plan cloud deployment** when ready for 24/7 operation

This multi-deployment approach gives you maximum flexibility while ensuring you can always fall back to your working Windows setup!