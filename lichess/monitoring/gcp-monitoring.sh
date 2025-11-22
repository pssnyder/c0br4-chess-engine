# GCP Cloud Monitoring Commands
# Use these commands to monitor your C0BR4 bot in Google Cloud Platform

# 1. Enable Cloud Monitoring API
gcloud services enable monitoring.googleapis.com

# 2. Create custom metrics (run these after deployment)
echo "Creating custom metrics for C0BR4 bot..."

# 3. Monitor CPU usage
gcloud logging metrics create c0br4_cpu_usage \
    --description="C0BR4 bot CPU usage" \
    --log-filter='resource.type="gce_instance" AND resource.labels.instance_name="c0br4-lichess-bot"'

# 4. Monitor memory usage  
gcloud logging metrics create c0br4_memory_usage \
    --description="C0BR4 bot memory usage" \
    --log-filter='resource.type="gce_instance" AND resource.labels.instance_name="c0br4-lichess-bot"'

# 5. Create alerting policy for high resource usage
gcloud alpha monitoring policies create --policy-from-file=monitoring/alerting-policy.yaml

# 6. View current resource usage
echo "Current instance metrics:"
gcloud compute instances describe c0br4-lichess-bot \
    --zone=us-central1-a \
    --format="table(machineType.scope(machineTypes):label=TYPE,status:label=STATUS)"

# 7. Get cost estimates
echo "Getting billing information..."
gcloud billing accounts list
gcloud billing projects describe $(gcloud config get-value project) --format="value(billingAccountName)"

# 8. Monitor network usage (important for cost)
gcloud compute instances get-serial-port-output c0br4-lichess-bot --zone=us-central1-a | tail -20

# 9. Set up budget alerts (replace PROJECT_ID and BILLING_ACCOUNT)
echo "Setting up budget alerts..."
echo "Visit: https://console.cloud.google.com/billing/budgets"
echo "Create budget: \$50/month with 50%, 90%, 100% alerts"

# 10. Continuous monitoring script
echo "For continuous monitoring, run:"
echo "watch -n 30 'gcloud compute instances describe c0br4-lichess-bot --zone=us-central1-a --format=\"table(status,machineType)\"'"