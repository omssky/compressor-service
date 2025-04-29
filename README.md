# Тестовый стенд в Minikube

Пошаговая инструкция по развёртыванию и мониторингу сервиса CompressorService в Minikube.

## 🚀 Запуск стенда

### 1. Подготовка Minikube
Инициализация кластера с указанными ресурсами:
```powershell
minikube delete
minikube start --container-runtime=containerd --driver=docker --cpus 10 --memory 10GB --disk-size 50GB
```

### 2. Настройка Docker-демона
Подключение к Docker-демону Minikube:
```powershell
minikube docker-env | Invoke-Expression
```

### 3. Сборка Docker-образа
Переход в директорию сервиса и сборка образа:
```powershell
cd src\CompressorService.Api
docker build -t compressor-service:dev .
```

### 4. Развёртывание в Kubernetes
Применение манифестов:
```powershell
cd ..\..
kubectl apply -f k8s/namespace.yaml
kubectl apply -n compressor -f k8s/deployment.yaml
kubectl apply -n compressor -f k8s/service.yaml
```

### 5. Проверка состояния
```powershell
kubectl get pods -n compressor
kubectl get svc -n compressor
```

## 📊 Мониторинг

### Установка Prometheus Stack
1. Добавление репозитория Helm:
```powershell
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
```

2. Установка в namespace `monitoring`:
```powershell
helm install prometheus-stack prometheus-community/kube-prometheus-stack --namespace monitoring --create-namespace --set alertmanager.enabled=false --set grafana.enabled=true --set kubernetesServiceMonitors.enabled=true --set nodeExporter.enabled=true --set prometheusOperator.enabled=true --set prometheus.enabled=true --set defaultRules.create=true  
```

3. Применение ServiceMonitor:
```powershell
kubectl apply -f k8s/servicemonitor.yaml
```

После этого Prometheus автоматически начнёт собирать метрики с `compressor-svc:5000/metrics`.

## 🔌 Port-forwarding

### Доступ к сервису
```powershell
kubectl port-forward -n compressor svc/compressor-svc 5000:5000 5002:5002
```
- Swagger UI: http://localhost:5000/swagger

### Prometheus UI
```powershell
kubectl port-forward -n monitoring svc/prometheus-stack-kube-prom-prometheus 9090:9090
```
- Интерфейс: http://localhost:9090

### Grafana
```powershell
kubectl port-forward -n monitoring svc/prometheus-stack-grafana 3000:80
```
- Интерфейс: http://localhost:3000