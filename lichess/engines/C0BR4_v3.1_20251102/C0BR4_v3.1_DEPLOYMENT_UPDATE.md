# C0BR4 v3.1 Lichess Deployment Update

**Date**: November 2, 2025  
**Update**: v2.9 → v3.1  
**Type**: Opening Book Improvements  

## 🚀 **Deployment Changes**

### **Engine Update**
- **Previous**: `engines/C0BR4_v2.9/` (v2.9)
- **Current**: `engines/C0BR4_v3.1/` (v3.1)
- **Executable**: `C0BR4_v3.1` (Linux self-contained)

### **Configuration Updates**
- **config-docker-cloud.yml**: Updated engine directory and name paths
- **Dockerfile**: Updated executable path for container permissions

## 🎯 **v3.1 Key Improvements**

### **Vienna Game Emergency Fix** 🚨
- **Fixed**: 75% defeat rate (worst performing opening)
- **Changed**: Replaced aggressive Vienna Gambit with safer Vienna System
- **Impact**: Expected 25% → 35%+ win rate improvement

### **Coverage Gap Additions** 📈
- **Added**: Queen's Pawn Game responses (83 games with 20.5% win rate)
- **Added**: Sicilian Defense responses (improved from 84.5% accuracy)
- **Added**: Anti-Torre Defense lines (fixed 70.8% accuracy issue)

### **Opening Book Statistics**
```
Previous (v2.9): 4 opening families, ~120 book moves
Current (v3.1):  6 opening families, 188 book moves (+57% expansion)
```

## 📋 **Deployment Checklist**

### **✅ Completed**
- [x] C0BR4_v3.1 engine copied to engines directory
- [x] config-docker-cloud.yml updated with new engine paths
- [x] Dockerfile updated with correct executable permissions
- [x] DEPLOYMENT_SUMMARY.md included for reference

### **🔄 Next Steps (Cloud Deployment)**
- [ ] Upload updated container to GCP
- [ ] Update cloud configuration
- [ ] Restart lichess bot service
- [ ] Monitor opening book behavior in first games
- [ ] Validate no regressions (UCI protocol, game handling)

## 🎮 **Expected Performance Changes**

### **Immediate Impact**
- **Vienna Game**: Stop playing disastrous f4 gambit lines
- **Queen's Pawn**: Better responses to Torre Attack systems
- **Sicilian Defense**: Improved counter-attacking repertoire

### **Validation Metrics**
- Monitor first 10-20 games for opening choices
- Check for elimination of Vienna Gambit (f4) moves
- Verify new opening book coverage in Queen's Pawn positions
- Overall win rate vs 1600-1800 opponents

## 🔧 **Technical Details**

### **Build Information**
- **Target**: Linux x64 (self-contained)
- **Dependencies**: None (all-in-one executable)
- **Size**: ~60MB (includes .NET runtime)
- **Protocol**: UCI 2.0 compatible

### **Container Changes**
```dockerfile
# OLD
RUN chmod +x engines/C0BR4_v2.9.exe

# NEW  
RUN chmod +x engines/C0BR4_v3.1/C0BR4_v3.1
```

### **Config Changes**
```yaml
# OLD
engine:
  dir: "./engines/c0br4/"
  name: "C0BR4_v2.9"

# NEW
engine:
  dir: "./engines/C0BR4_v3.1/"
  name: "C0BR4_v3.1"
```

---

**Ready for GCP cloud deployment and lichess bot restart** 🎯  
**Expected minimal disruption - opening book changes only**  
**Rollback available via v2.9 engine if needed**