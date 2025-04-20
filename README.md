## Пошаговая инструкция по запуску тестового стенда в Minikube

1. **Подготовка Minikube**.

   Используем Docker-драйвер и задаём ресурсы

   ```powershell
   minikube config set driver docker
   minikube config set cpus 10
   minikube config set memory 10000
   minikube config set disk-size 50g
   ```

   Перезапускаем кластер

   ```powershell
   minikube delete
   minikube start --addons=metrics-server
   ```

2. **Подключаемся к Docker‑демону Minikube**

   ```powershell
   minikube docker-env | Invoke-Expression
   ```

3. **Собираем Docker‑образ сервиса**

   ```powershell
   cd src\CompressorService.Api
   docker build -t compressor-service:dev .
   ```

4. **Применяем манифесты**

   ```powershell
   cd ..\..              # возвращаемся в корень compressor-service/
   kubectl apply -f k8s/namespace.yaml
   kubectl apply -n compressor -f k8s/deployment.yaml
   kubectl apply -n compressor -f k8s/service.yaml
   ```

5. **Проверяем статус**

   ```powershell
   kubectl get pods -n compressor
   kubectl get svc -n compressor
   kubectl top pods -n compressor
   ```

6. **Пробрасываем порты**

   - **Port‑forward** (ибо NodePort из под docker не работает):
     ```powershell
     kubectl port-forward -n compressor svc/compressor-svc 5000:5000 5002:5002
     ```
     → `http://localhost:5000/swagger`  
     → gRPC на `localhost:5002`

7. **Проверка метрик**

   - CPU/Memory:
     ```powershell
     kubectl top nodes
     kubectl top pods -n compressor
     ```
   - `/metrics`:
     ```
     curl http://localhost:5000/metrics | head
     ```

8. **Мониторинг с Prometheus Operator и Grafana**

   - Установка Prometheus Operator
     добавляем репозиторий и обновляем индексы

     ```powershell
     helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
     helm repo update
     ```

   - Устанавливаем в namespace monitoring (создаётся автоматически)

     ```powershell
     helm install prometheus-stack prometheus-community/kube-prometheus-stack --namespace monitoring --create-namespace
     ```

   - Применяем ServiceMonitor:

     ```powershell
     kubectl apply -f k8s/servicemonitor.yaml
     ```

     После этого Prometheus Operator автоматически подхватит ServiceMonitor и начнёт скрейпить compressor-svc:5000/metrics.

## Port‑forwarding

### Prometheus UI

```powershell
kubectl port-forward -n monitoring svc/prometheus-stack-kube-prom-prometheus 9090:9090
```

http://localhost:9090

### Grafana

```powershell
kubectl port-forward -n monitoring svc/prometheus-stack-grafana 3000:80
```

http://localhost:3000

Login/Password: admin / prom-operator (пароль можно получить из секрета prometheus-stack-grafana).
