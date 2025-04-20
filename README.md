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

6. **Доступ к Swagger‑UI и gRPC**

    - **NodePort** (использовать напрямую без port‑forward):
        1. Узнать IP Minikube:
           ```powershell
           minikube ip    # например 192.168.49.2
           ```  
        2. Открыть в браузере:
           ```
           http://192.168.49.2:30080/swagger
           ```
        3. gRPC‑адрес для тестов:
           ```
           grpc://192.168.49.2:30082
           ```

    - **Port‑forward** (если NodePort недоступен):
      ```powershell
      kubectl port-forward -n compressor svc/compressor-svc 5000:80 5002:5002
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
