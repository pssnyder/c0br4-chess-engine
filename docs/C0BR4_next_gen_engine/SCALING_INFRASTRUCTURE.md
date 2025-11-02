# Dynamic Scaling & Infrastructure Guide

## 🎯 **Overview**

This guide details the implementation of intelligent resource scaling for C0BR4, enabling the chess engine to automatically adapt computational resources based on game importance, opponent strength, and tournament context.

## 📋 **Table of Contents**

1. [Scaling Philosophy](#scaling-philosophy)
2. [Resource Tier Architecture](#resource-tier-architecture)
3. [Auto-Scaling Implementation](#auto-scaling-implementation)
4. [Tournament Detection](#tournament-detection)
5. [Cost Management](#cost-management)
6. [Infrastructure as Code](#infrastructure-as-code)

---

## 🎭 **Scaling Philosophy**

### **Core Principle: Contextual Resource Allocation**

The system operates on the principle that **not all chess games are created equal**. Different game contexts require different computational approaches:

```yaml
Game Context Hierarchy:
  🥇 Tournament Games: Maximum resources, full AI assistance
  🥈 Rated Competitive: Balanced resources, complexity AI
  🥉 Casual Learning: Minimal resources, conservation mode
```

### **Grandmaster-Inspired Strategy**

Just as grandmaster players allocate mental energy based on game importance:
- **Critical games**: Full concentration and time
- **Practice games**: Efficient play, pattern recognition
- **Exhibition games**: Relaxed approach, experimentation

---

## ⚡ **Resource Tier Architecture**

### **Tier 1: Conservation Mode** 💚

#### **Target Use Cases**
```yaml
Games: Casual play, learning games, rating building
Opponent Rating: <1800
Game Type: Casual, unrated
Expected Volume: 70% of all games
Philosophy: "Efficient learning with minimal cost"
```

#### **Resource Allocation**
```yaml
Cloud Instance: e2-small (2 vCPU, 2GB RAM)
Monthly Cost: ~$12 (base tier)
Neural Networks: None (pure engine)
Search Configuration:
  - Base depth: 6 ply
  - Time per move: Conservative
  - Evaluation: Standard
  - Opening book: Basic
```

#### **Engine Configuration**
```yaml
conservation_mode:
  engine:
    search_depth: 6
    time_multiplier: 1.0
    hash_size: 128  # MB
    threads: 1
    
  strategy:
    accept_losses: true  # Focus on learning
    play_for_analysis: true
    avoid_long_games: false
    
  neural_networks:
    complexity_assessment: false
    opening_endgame: false
```

### **Tier 2: Competition Mode** 🟡

#### **Target Use Cases**
```yaml
Games: Rated games, league play, strong casual opponents
Opponent Rating: 1800-2200
Game Type: Rated or strong casual
Expected Volume: 25% of all games
Philosophy: "Balanced performance and efficiency"
```

#### **Resource Allocation**
```yaml
Cloud Instance: e2-medium (2 vCPU, 4GB RAM)
Monthly Cost: ~$25 (scaled during usage)
Neural Networks: Complexity Assessment active
Search Configuration:
  - Base depth: 8 ply
  - Time per move: Complexity-adjusted
  - Evaluation: Enhanced
  - Opening book: Comprehensive
```

#### **Engine Configuration**
```yaml
competition_mode:
  engine:
    search_depth: 8
    time_multiplier: 1.5
    hash_size: 512  # MB
    threads: 2
    
  strategy:
    play_for_rating: true
    careful_time_management: true
    avoid_risky_lines: false
    
  neural_networks:
    complexity_assessment: true
    opening_endgame: false
```

### **Tier 3: Tournament Mode** 🔴

#### **Target Use Cases**
```yaml
Games: Official tournaments, titled opponents, critical matches
Opponent Rating: >2200 or titled players
Game Type: Tournament, match play
Expected Volume: 5% of all games
Philosophy: "Maximum performance regardless of cost"
```

#### **Resource Allocation**
```yaml
Cloud Instance: c2-standard-8 (8 vCPU, 32GB RAM)
Monthly Cost: ~$200 (only during tournaments)
Neural Networks: All systems active
Search Configuration:
  - Base depth: 12+ ply
  - Time per move: Complexity + importance adjusted
  - Evaluation: Full analysis
  - Opening book: Tournament preparation
```

#### **Engine Configuration**
```yaml
tournament_mode:
  engine:
    search_depth: 12
    time_multiplier: 2.5
    hash_size: 2048  # MB
    threads: 8
    
  strategy:
    play_for_win: true
    maximum_analysis: true
    use_all_time: true
    
  neural_networks:
    complexity_assessment: true
    opening_endgame: true
    advanced_features: true
```

---

## 🤖 **Auto-Scaling Implementation**

### **Game Context Analyzer**

```python
class GameContextAnalyzer:
    def __init__(self):
        self.lichess_api = LichessAPI()
        self.tournament_monitor = TournamentMonitor()
        
    def analyze_game_context(self, challenge):
        """Analyze incoming challenge and determine resource requirements"""
        
        context = GameContext(
            opponent_rating=challenge.challenger.rating,
            opponent_title=challenge.challenger.title,
            game_type=challenge.variant,
            time_control=challenge.clock,
            is_rated=challenge.rated
        )
        
        # Calculate game importance score
        importance = self.calculate_importance_score(context)
        
        # Determine resource tier
        tier = self.select_resource_tier(importance, context)
        
        return {
            'context': context,
            'importance_score': importance,
            'resource_tier': tier,
            'estimated_cost': self.estimate_game_cost(tier),
            'scaling_recommendation': self.get_scaling_recommendation(tier)
        }
        
    def calculate_importance_score(self, context):
        """Calculate game importance (0.0 - 1.0)"""
        score = 0.0
        
        # Tournament games are automatically high importance
        if self.tournament_monitor.is_tournament_active():
            score += 0.6
            
        # Rating-based importance
        if context.opponent_rating > 2200:
            score += 0.3
        elif context.opponent_rating > 1800:
            score += 0.15
            
        # Title-based importance
        title_bonuses = {'GM': 0.3, 'IM': 0.2, 'FM': 0.1, 'NM': 0.05}
        score += title_bonuses.get(context.opponent_title, 0)
        
        # Game type importance
        if context.is_rated:
            score += 0.1
            
        # Time control importance (longer games = more important)
        if context.time_control.base_minutes > 15:
            score += 0.1
            
        return min(1.0, score)
        
    def select_resource_tier(self, importance, context):
        """Select appropriate resource tier based on importance"""
        
        if importance >= 0.7:
            return "tournament_mode"
        elif importance >= 0.3:
            return "competition_mode"
        else:
            return "conservation_mode"
```

### **Scaling Orchestrator**

```python
class ScalingOrchestrator:
    def __init__(self):
        self.cloud_manager = CloudResourceManager()
        self.cost_monitor = CostMonitor()
        self.current_tier = "conservation_mode"
        
    async def handle_scaling_request(self, target_tier, game_context):
        """Handle request to scale resources"""
        
        # Check if scaling is needed
        if target_tier == self.current_tier:
            return {"action": "no_scaling_needed", "current_tier": self.current_tier}
            
        # Validate scaling request against budget
        scaling_approved = await self.validate_scaling_request(target_tier, game_context)
        
        if not scaling_approved:
            return {
                "action": "scaling_denied", 
                "reason": "budget_constraints",
                "fallback_tier": self.current_tier
            }
            
        # Execute scaling
        scaling_result = await self.execute_scaling(target_tier)
        
        if scaling_result["success"]:
            self.current_tier = target_tier
            return {
                "action": "scaling_completed",
                "new_tier": target_tier,
                "instance_type": scaling_result["instance_type"],
                "estimated_cost": scaling_result["estimated_cost"]
            }
        else:
            return {
                "action": "scaling_failed",
                "error": scaling_result["error"],
                "fallback_tier": self.current_tier
            }
            
    async def validate_scaling_request(self, target_tier, game_context):
        """Validate scaling request against budget and policies"""
        
        # Get cost estimate
        estimated_cost = self.estimate_scaling_cost(target_tier, game_context)
        
        # Check budget constraints
        budget_check = await self.cost_monitor.approve_spending(estimated_cost)
        
        if not budget_check["approved"]:
            return False
            
        # Special validation for tournament mode
        if target_tier == "tournament_mode":
            # Require explicit tournament context
            if not game_context.is_tournament:
                # Allow for titled opponents or high-rated games
                if game_context.opponent_rating < 2200 and not game_context.opponent_title:
                    return False
                    
        return True
        
    async def execute_scaling(self, target_tier):
        """Execute the actual scaling operation"""
        
        tier_configs = {
            "conservation_mode": {
                "instance_type": "e2-small",
                "neural_networks": [],
                "engine_config": "conservative"
            },
            "competition_mode": {
                "instance_type": "e2-medium", 
                "neural_networks": ["complexity"],
                "engine_config": "balanced"
            },
            "tournament_mode": {
                "instance_type": "c2-standard-8",
                "neural_networks": ["complexity", "opening_endgame"],
                "engine_config": "maximum"
            }
        }
        
        config = tier_configs[target_tier]
        
        try:
            # Scale cloud instance
            instance_result = await self.cloud_manager.scale_instance(
                instance_type=config["instance_type"]
            )
            
            # Update engine configuration
            engine_result = await self.update_engine_config(
                config=config["engine_config"],
                neural_networks=config["neural_networks"]
            )
            
            return {
                "success": True,
                "instance_type": config["instance_type"],
                "estimated_cost": self.calculate_tier_cost(target_tier)
            }
            
        except Exception as e:
            return {"success": False, "error": str(e)}
```

---

## 🏆 **Tournament Detection**

### **Lichess Tournament API Integration**

```python
class TournamentMonitor:
    def __init__(self):
        self.lichess_api = LichessAPI()
        self.active_tournaments = {}
        self.monitoring_interval = 300  # 5 minutes
        
    async def start_monitoring(self):
        """Start continuous tournament monitoring"""
        while True:
            await self.check_tournaments()
            await asyncio.sleep(self.monitoring_interval)
            
    async def check_tournaments(self):
        """Check for new tournaments and registrations"""
        
        # Get current tournaments
        current_tournaments = await self.lichess_api.get_tournaments()
        
        for tournament in current_tournaments:
            if self.should_participate(tournament):
                registration_result = await self.register_for_tournament(tournament)
                
                if registration_result["success"]:
                    # Trigger scaling to tournament mode
                    await self.prepare_for_tournament(tournament)
                    
    def should_participate(self, tournament):
        """Determine if bot should participate in tournament"""
        
        criteria = {
            'min_time_control': 180,  # 3+ minute games
            'max_participants': 500,  # Manageable size
            'min_rating_range': 1500,  # Appropriate competition
            'max_rating_range': 2500,
            'allowed_variants': ['standard', 'chess960']
        }
        
        return (
            tournament.time_control >= criteria['min_time_control'] and
            tournament.participant_count <= criteria['max_participants'] and
            criteria['min_rating_range'] <= tournament.avg_rating <= criteria['max_rating_range'] and
            tournament.variant in criteria['allowed_variants']
        )
        
    async def prepare_for_tournament(self, tournament):
        """Prepare resources for tournament participation"""
        
        # Calculate tournament duration and estimated games
        tournament_duration = tournament.end_time - tournament.start_time
        estimated_games = min(tournament_duration.seconds // 300, 50)  # Estimate
        
        # Trigger scaling to tournament mode
        scaling_request = {
            'target_tier': 'tournament_mode',
            'reason': 'tournament_participation',
            'tournament_id': tournament.id,
            'estimated_duration': tournament_duration,
            'estimated_games': estimated_games
        }
        
        await self.scaling_orchestrator.handle_scaling_request(scaling_request)
        
        # Update engine configuration for tournament
        await self.configure_engine_for_tournament(tournament)
        
    async def configure_engine_for_tournament(self, tournament):
        """Configure engine specifically for tournament play"""
        
        tournament_config = {
            'time_management': 'aggressive',
            'opening_preparation': tournament.variant,
            'neural_networks': ['complexity', 'opening_endgame'],
            'search_depth': 'maximum',
            'evaluation': 'comprehensive'
        }
        
        await self.engine_manager.update_configuration(tournament_config)
```

### **Predictive Tournament Scaling**

```python
class PredictiveScaler:
    def __init__(self):
        self.tournament_history = TournamentHistory()
        self.pattern_analyzer = PatternAnalyzer()
        
    def predict_upcoming_tournaments(self):
        """Predict likely tournament participation based on patterns"""
        
        # Analyze historical patterns
        patterns = self.pattern_analyzer.analyze_tournament_patterns(
            self.tournament_history.get_last_30_days()
        )
        
        predictions = []
        
        for pattern in patterns:
            if pattern.confidence > 0.7:
                predictions.append({
                    'predicted_time': pattern.next_occurrence,
                    'tournament_type': pattern.tournament_type,
                    'confidence': pattern.confidence,
                    'recommended_preparation': pattern.preparation_strategy
                })
                
        return predictions
        
    async def pre_scale_for_predictions(self, predictions):
        """Pre-scale resources based on tournament predictions"""
        
        for prediction in predictions:
            time_until_tournament = prediction['predicted_time'] - datetime.now()
            
            # Pre-scale 30 minutes before predicted tournament
            if timedelta(minutes=25) <= time_until_tournament <= timedelta(minutes=35):
                await self.scaling_orchestrator.handle_scaling_request({
                    'target_tier': 'competition_mode',  # Pre-scale to medium tier
                    'reason': 'predictive_scaling',
                    'confidence': prediction['confidence']
                })
```

---

## 💰 **Cost Management**

### **Budget-Aware Scaling**

```python
class CostMonitor:
    def __init__(self, monthly_budget=50):
        self.monthly_budget = monthly_budget
        self.current_spending = 0
        self.spending_history = []
        self.cost_thresholds = {
            'warning': 0.7,    # 70% of budget
            'critical': 0.9,   # 90% of budget
            'emergency': 0.95  # 95% of budget
        }
        
    async def approve_spending(self, estimated_cost):
        """Approve or deny spending request based on budget"""
        
        projected_spending = self.current_spending + estimated_cost
        budget_utilization = projected_spending / self.monthly_budget
        
        if budget_utilization < self.cost_thresholds['warning']:
            return {"approved": True, "reason": "within_budget"}
            
        elif budget_utilization < self.cost_thresholds['critical']:
            # Conditional approval for important games
            return {"approved": True, "reason": "budget_warning", "warning": True}
            
        elif budget_utilization < self.cost_thresholds['emergency']:
            # Only approve for tournaments
            return {"approved": False, "reason": "budget_critical"}
            
        else:
            # Emergency stop - no more scaling
            return {"approved": False, "reason": "budget_exceeded"}
            
    def calculate_cost_per_game(self):
        """Calculate average cost per game for optimization"""
        
        if len(self.spending_history) == 0:
            return 0
            
        total_cost = sum(record['cost'] for record in self.spending_history)
        total_games = sum(record['games'] for record in self.spending_history)
        
        return total_cost / total_games if total_games > 0 else 0
        
    def optimize_tier_allocation(self):
        """Optimize resource tier allocation based on cost efficiency"""
        
        cost_per_game = self.calculate_cost_per_game()
        
        if cost_per_game > 0.5:  # $0.50 per game
            return {
                'recommendation': 'reduce_tournament_mode_usage',
                'suggested_changes': [
                    'Increase tournament mode threshold',
                    'Use competition mode for rated games only',
                    'Implement stricter budget controls'
                ]
            }
        elif cost_per_game < 0.1:  # Very efficient
            return {
                'recommendation': 'increase_performance_tier_usage',
                'suggested_changes': [
                    'Lower tournament mode threshold',
                    'Use competition mode more frequently',
                    'Consider higher performance instances'
                ]
            }
        else:
            return {'recommendation': 'maintain_current_allocation'}
```

### **Dynamic Pricing Strategy**

```python
class DynamicPricingManager:
    def __init__(self):
        self.cloud_pricing = CloudPricingAPI()
        self.preemptible_risk_tolerance = 0.8
        
    async def optimize_instance_selection(self, target_tier, duration_estimate):
        """Select optimal instance type based on current pricing"""
        
        instance_options = await self.cloud_pricing.get_current_pricing()
        
        # Consider preemptible instances for cost savings
        for option in instance_options:
            if option.tier == target_tier:
                
                # For short games, prefer preemptible (60-90% discount)
                if duration_estimate < timedelta(hours=2):
                    if option.preemptible_available:
                        return {
                            'instance_type': option.preemptible_type,
                            'cost_savings': option.preemptible_discount,
                            'interruption_risk': option.interruption_probability
                        }
                        
                # For tournaments, prefer standard instances
                elif target_tier == 'tournament_mode':
                    return {
                        'instance_type': option.standard_type,
                        'cost_premium': option.reliability_premium,
                        'interruption_risk': 0
                    }
                    
        return self.get_default_instance(target_tier)
```

---

## 🏗️ **Infrastructure as Code**

### **Terraform Configuration**

```hcl
# terraform/main.tf
variable "environment" {
  description = "Environment (dev/staging/prod)"
  type        = string
  default     = "prod"
}

variable "monthly_budget" {
  description = "Monthly budget in USD"
  type        = number
  default     = 50
}

# Instance configurations for different tiers
locals {
  instance_configs = {
    conservation = {
      machine_type = "e2-small"
      disk_size    = 20
      preemptible  = true
    }
    competition = {
      machine_type = "e2-medium"
      disk_size    = 30
      preemptible  = false
    }
    tournament = {
      machine_type = "c2-standard-8"
      disk_size    = 50
      preemptible  = false
    }
  }
}

# Managed Instance Group for auto-scaling
resource "google_compute_instance_template" "c0br4_template" {
  count = 3  # One for each tier
  
  name_prefix  = "c0br4-${keys(local.instance_configs)[count.index]}-"
  machine_type = values(local.instance_configs)[count.index].machine_type
  
  disk {
    source_image = "cos-cloud/cos-stable"
    auto_delete  = true
    boot         = true
    disk_size_gb = values(local.instance_configs)[count.index].disk_size
  }
  
  network_interface {
    network = "default"
    access_config {}
  }
  
  metadata = {
    "gce-container-declaration" = file("${path.module}/container-configs/tier-${keys(local.instance_configs)[count.index]}.yaml")
    "google-logging-enabled"    = "true"
  }
  
  scheduling {
    automatic_restart   = !values(local.instance_configs)[count.index].preemptible
    on_host_maintenance = values(local.instance_configs)[count.index].preemptible ? "TERMINATE" : "MIGRATE"
    preemptible         = values(local.instance_configs)[count.index].preemptible
  }
  
  lifecycle {
    create_before_destroy = true
  }
}

# Budget alerts
resource "google_billing_budget" "c0br4_budget" {
  billing_account = var.billing_account
  display_name    = "C0BR4 Chess Bot Budget"
  
  amount {
    specified_amount {
      currency_code = "USD"
      units         = var.monthly_budget
    }
  }
  
  threshold_rules {
    threshold_percent = 0.5
    spend_basis       = "CURRENT_SPEND"
  }
  
  threshold_rules {
    threshold_percent = 0.9
    spend_basis       = "CURRENT_SPEND"
  }
  
  all_updates_rule {
    monitoring_notification_channels = [
      google_monitoring_notification_channel.email.id
    ]
  }
}
```

### **Kubernetes Deployment (Alternative)**

```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: c0br4-chess-bot
  namespace: chess-bots
spec:
  replicas: 1
  selector:
    matchLabels:
      app: c0br4-chess-bot
  template:
    metadata:
      labels:
        app: c0br4-chess-bot
    spec:
      containers:
      - name: c0br4-bot
        image: gcr.io/PROJECT_ID/c0br4-lichess-bot:latest
        env:
        - name: LICHESS_TOKEN
          valueFrom:
            secretKeyRef:
              name: lichess-token
              key: token
        - name: RESOURCE_TIER
          value: "conservation_mode"
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "2000m"
        volumeMounts:
        - name: game-records
          mountPath: /lichess-bot/game_records
      volumes:
      - name: game-records
        persistentVolumeClaim:
          claimName: c0br4-game-records

---
# Horizontal Pod Autoscaler for tournament mode
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: c0br4-hpa
  namespace: chess-bots
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: c0br4-chess-bot
  minReplicas: 1
  maxReplicas: 3
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Pods
    pods:
      metric:
        name: active_games_count
      target:
        type: AverageValue
        averageValue: "2"
```

---

This scaling infrastructure enables C0BR4 to **intelligently adapt** its computational resources based on game context, providing grandmaster-level performance when it matters most while maintaining cost efficiency for routine play.