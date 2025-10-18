pipeline {
    agent any

    environment {
        // .NET Configuration
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "1"
        PATH = "$HOME/.dotnet:${env.PATH}"
        DOTNET_CLI_TELEMETRY_OPTOUT = "1"
        
        // Liquibase Configuration
        LIQUIBASE_HOME = "$WORKSPACE/.liquibase"

        // Database Configuration
        DB_CONNECTION_STRING = "server=163.223.210.80;port=3306;database=IGCSE;user=root;password=rootpassword;TreatTinyAsBoolean=true;Allow User Variables=true;SslMode=None;AllowPublicKeyRetrieval=True"
        ConnectionStrings__DbConnection = "server=163.223.210.80;port=3306;database=IGCSE;user=root;password=rootpassword;TreatTinyAsBoolean=true;Allow User Variables=true;SslMode=None;AllowPublicKeyRetrieval=True"

        // JWT Configuration
        JWT__Issuer = ""
        JWT__Audience = ""
        JWT__SigningKey = "sdgfijjh3466iu345g87g08c24g7204gr803g30587ghh35807fg39074fvg80493745gf082b507807g807fgf"
    }

    stages {

        /* =====================================
           🧩 CHECK WORKSPACE
        ====================================== */
        stage('Check Workspace') {
            steps {
                sh '''
                    echo "=== CURRENT DIRECTORY ==="
                    pwd

                    echo "=== LISTING ALL FILES ==="
                    ls -la

                    echo "=== FINDING PROJECT FILES ==="
                    find . -name "*.csproj" -o -name "*.sln" | head -10
                '''
            }
        }

        /* =====================================
           ⚙️ INSTALL .NET SDK
        ====================================== */
        stage('Install .NET SDK') {
            steps {
                sh '''
                    echo "=== CHECKING .NET SDK INSTALLATION ==="

                    if command -v dotnet >/dev/null 2>&1; then
                        echo "✅ .NET SDK already installed:"
                        dotnet --info | head -n 10
                    else
                        echo "⚙️ Installing .NET SDK..."
                        curl -L https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
                        chmod +x dotnet-install.sh
                        ./dotnet-install.sh --channel 8.0 --install-dir $HOME/.dotnet
                    fi

                    export DOTNET_ROOT=$HOME/.dotnet
                    export PATH=$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH

                    echo "=== VERIFY .NET INSTALL ==="
                    dotnet --info
                    dotnet --list-runtimes
                    dotnet --list-sdks

                    echo "=== INSTALL EF CLI ==="
                    if [ ! -f "$DOTNET_ROOT/tools/dotnet-ef" ]; then
                        mkdir -p "$DOTNET_ROOT/tools"
                        dotnet tool install --tool-path "$DOTNET_ROOT/tools" dotnet-ef || true
                    fi

                    "$DOTNET_ROOT/tools/dotnet-ef" --version || true
                '''
            }
        }

        /* =====================================
           🗃️ RUN EF MIGRATIONS
        ====================================== */
        stage('Run EF Migrations') {
            steps {
                sh '''
                    echo "=== INSTALL DEPENDENCIES ==="
                    if [ -f /etc/alpine-release ]; then
                        apk add --no-cache bash icu-libs curl
                    else
                        if command -v apt-get >/dev/null 2>&1 && [ "$(id -u)" -eq 0 ]; then
                            apt-get update && apt-get install -y curl
                        fi
                    fi

                    echo "=== SETUP .NET SDK ==="
                    curl -L https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
                    chmod +x dotnet-install.sh
                    ./dotnet-install.sh --version 8.0.401 --install-dir $HOME/.dotnet

                    export DOTNET_ROOT="$HOME/.dotnet"
                    export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
                    export LD_LIBRARY_PATH="$DOTNET_ROOT:${LD_LIBRARY_PATH:-}"
                    export DOTNET_CLI_TELEMETRY_OPTOUT=1

                    echo "=== INSTALL EF TOOL IF NEEDED ==="
                    mkdir -p "$DOTNET_ROOT/tools"
                    dotnet tool install --tool-path "$DOTNET_ROOT/tools" dotnet-ef || true
                    "$DOTNET_ROOT/tools/dotnet-ef" --version || true

                    echo "=== COPY APPSETTINGS ==="
                    cp ./IGCSE/appsettings.json ./BusinessObject/appsettings.json || true

                    echo "=== RUN EF MIGRATION ==="
                    "$DOTNET_ROOT/tools/dotnet-ef" database update \
                        --project ./BusinessObject/BusinessObject.csproj \
                        --startup-project ./IGCSE/IGCSE.csproj \
                        --connection "$DB_CONNECTION_STRING" \
                        --verbose
                '''
            }
        }

        /* =====================================
           💾 RUN LIQUIBASE
        ====================================== */
        stage('Run Liquibase') {
            steps {
                sh '''
                    echo "=== RUNNING LIQUIBASE MIGRATIONS ==="

                    if [ -f /etc/alpine-release ]; then
                        apk add --no-cache unzip curl || true
                    else
                        if command -v apt-get >/dev/null 2>&1 && [ "$(id -u)" -eq 0 ]; then
                            apt-get update && apt-get install -y unzip curl || true
                        fi
                    fi

                    if [ ! -f "$LIQUIBASE_HOME/liquibase" ]; then
                        echo "Installing Liquibase..."
                        mkdir -p "$LIQUIBASE_HOME"
                        curl -L https://github.com/liquibase/liquibase/releases/download/v4.29.2/liquibase-4.29.2.zip -o /tmp/liquibase.zip
                        unzip -o /tmp/liquibase.zip -d "$LIQUIBASE_HOME" >/dev/null 2>&1

                        if [ ! -f "$LIQUIBASE_HOME/liquibase" ]; then
                            inner=$(find "$LIQUIBASE_HOME" -maxdepth 1 -type d -name "liquibase-*" | head -1)
                            if [ -n "$inner" ]; then
                                mv "$inner"/* "$LIQUIBASE_HOME"/
                            fi
                        fi

                        chmod +x "$LIQUIBASE_HOME/liquibase" || true
                    fi

                    cd Migration
                    echo "Using defaultsFile=$(pwd)/liquibase.properties"
                    $LIQUIBASE_HOME/liquibase --defaultsFile=liquibase.properties update

                    echo "✅ Liquibase migrations applied successfully."
                '''
            }
        }

        /* =====================================
           🏗️ BUILD APPLICATION
        ====================================== */
        stage('Build Application') {
            steps {
                sh '''
                    echo "=== COPY APPSETTINGS ==="
                    mkdir -p ./BusinessObject
                    cp ./IGCSE/appsettings.json ./BusinessObject/appsettings.json || true

                    echo "=== BUILD PROCESS ==="
                    SOLUTION_FILE=$(find . -name "IGCSE.sln" | head -1)

                    if [ -n "$SOLUTION_FILE" ]; then
                        echo "✅ Found solution file: $SOLUTION_FILE"
                        dotnet restore "$SOLUTION_FILE"
                        dotnet build "$SOLUTION_FILE" --configuration Release

                        PROJECT_FILE=$(find . -name "IGCSE.csproj" | head -1)
                        if [ -n "$PROJECT_FILE" ]; then
                            echo "📦 Publishing project: $PROJECT_FILE"
                            dotnet publish "$PROJECT_FILE" -c Release -o ./publish
                        else
                            echo "❌ Project file not found!"
                            exit 1
                        fi
                    else
                        echo "❌ No solution file found!"
                        exit 1
                    fi

                    echo "=== PUBLISHED OUTPUT ==="
                    ls -la ./publish/
                '''
            }
        }

        /* =====================================
           🧪 SMOKE TEST
        ====================================== */
        stage('Smoke Test') {
            steps {
                sh '''
                    echo "=== RUNNING SMOKE TEST ==="
                    DLL_FILE=$(find ./publish -maxdepth 1 -name "IGCSE.dll" | head -1)

                    if [ -z "$DLL_FILE" ]; then
                        echo "❌ ERROR: IGCSE.dll not found!"
                        exit 1
                    fi

                    echo "✅ Found DLL file: $DLL_FILE"
                    export ASPNETCORE_URLS="http://0.0.0.0:7211"
                    nohup dotnet "$DLL_FILE" > smoke_test.log 2>&1 &
                    APP_PID=$!
                    sleep 7

                    if ! ps -p $APP_PID > /dev/null; then
                        echo "❌ App failed to start"
                        cat smoke_test.log
                        exit 1
                    fi

                    echo "✅ App started successfully"
                    RESPONSE=$(curl -s -o response.json -w "%{http_code}" http://localhost:7211/api/ping || true)

                    if [ "$RESPONSE" != "200" ]; then
                        echo "❌ Smoke test failed — /ping không trả về 200"
                        kill $APP_PID || true
                        exit 1
                    fi

                    EXPECTED='{"message":"pong"}'
                    ACTUAL=$(cat response.json | tr -d '[:space:]')

                    if [ "$ACTUAL" != "$EXPECTED" ]; then
                        echo "❌ Smoke test failed — Response không khớp"
                        echo "Expected: $EXPECTED"
                        echo "Actual:   $ACTUAL"
                        kill $APP_PID || true
                        exit 1
                    fi

                    echo "✅ Smoke test passed!"
                    kill $APP_PID || true
                '''
            }
        }

        /* =====================================
           🚀 RUN APP
        ====================================== */
        stage('Run App') {
            steps {
                sh '''
                    echo "🚀 Starting app in background..."
                    pkill -f "dotnet ./publish/IGCSE.dll" || true
                    export ASPNETCORE_URLS="http://0.0.0.0:7211"
                    setsid nohup dotnet ./publish/IGCSE.dll > app.out 2>&1 < /dev/null &
                    disown
                    sleep 5
                    echo "✅ App started on port 7211"
                    ps aux | grep "IGCSE.dll" | grep -v grep
                '''
            }
        }

        /* =====================================
           📦 DEPLOY LOCAL
        ====================================== */
        stage('Deploy Local') {
            steps {
                sh '''
                    echo "=== DEPLOYING LOCALLY ==="
                    mkdir -p ./deploy
                    cp -r ./publish/* ./deploy/
                    chmod -R 755 ./deploy
                    echo "✅ LOCAL DEPLOYMENT COMPLETED!"
                    ls -la ./deploy/
                '''
            }
        }

        /* =====================================
           🐳 BUILD DOCKER IMAGE
        ====================================== */
        stage('Build Docker Image') {
            steps {
                sh '''
                    echo "=== BUILDING DOCKER IMAGE ==="
                    
                    # Create Dockerfile if not exists
                    if [ ! -f Dockerfile ]; then
                        cat > Dockerfile << 'EOF'
# ===========================================
# Build Stage
# ===========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution context
COPY ./BusinessObject ./BusinessObject
COPY ./Common ./Common
COPY ./Repository ./Repository
COPY ./Service ./Service
COPY ./Migration ./Migration
COPY ./IGCSE ./IGCSE

# Restore dependencies
RUN dotnet restore ./IGCSE/IGCSE.csproj

# Build and publish
RUN dotnet publish ./IGCSE/IGCSE.csproj -c Release -o /app/publish /p:UseAppHost=false

# ===========================================
# Runtime Stage
# ===========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy built application
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:7211
EXPOSE 7211

# Start application
ENTRYPOINT ["dotnet", "IGCSE.dll"]
EOF
                    fi

                    # Build Docker image
                    docker build -t igcse-app:${BUILD_NUMBER} .
                    docker tag igcse-app:${BUILD_NUMBER} igcse-app:latest
                    
                    echo "✅ Docker image built successfully"
                    docker images | grep igcse-app
                '''
            }
        }

        /* =====================================
           🚀 DEPLOY TO SERVER
        ====================================== */
        stage('Deploy to Server') {
            steps {
                sh '''
                    echo "=== DEPLOYING TO SERVER ==="
                    
                    # Create deployment script
                    cat > deploy.sh << 'EOF'
#!/bin/bash
set -e

echo "=== STOPPING OLD CONTAINER ==="
docker stop igcse-app || true
docker rm igcse-app || true

echo "=== PULLING NEW IMAGE ==="
# If using registry, pull here
# docker pull your-registry/igcse-app:latest

echo "=== STARTING NEW CONTAINER ==="
docker run -d \
    --name igcse-app \
    --restart unless-stopped \
    -p 7211:7211 \
    -e ConnectionStrings__DbConnection="$DB_CONNECTION_STRING" \
    -e JWT__Issuer="$JWT__Issuer" \
    -e JWT__Audience="$JWT__Audience" \
    -e JWT__SigningKey="$JWT__SigningKey" \
    -e ASPNETCORE_ENVIRONMENT=Production \
    -e ASPNETCORE_URLS=http://+:7211 \
    igcse-app:latest

echo "=== WAITING FOR APP TO START ==="
sleep 10

echo "=== CHECKING APP HEALTH ==="
for i in {1..30}; do
    if curl -f http://localhost:7211/api/ping >/dev/null 2>&1; then
        echo "✅ App is healthy!"
        break
    fi
    echo "Waiting for app... ($i/30)"
    sleep 2
done

echo "=== DEPLOYMENT COMPLETED ==="
docker ps | grep igcse-app
EOF

                    chmod +x deploy.sh
                    
                    # Deploy locally (for testing)
                    ./deploy.sh
                    
                    echo "✅ DEPLOYMENT COMPLETED!"
                '''
            }
        }

        /* =====================================
           🔄 DEPLOY WITH DOCKER COMPOSE
        ====================================== */
        stage('Deploy with Docker Compose') {
            when {
                expression { env.BRANCH_NAME == 'main' || env.BRANCH_NAME == 'master' }
            }
            steps {
                sh '''
                    echo "=== DEPLOYING WITH DOCKER COMPOSE ==="
                    
                    # Create docker-compose.yml
                    cat > docker-compose.yml << 'EOF'
                        version: "3.9"

services:
  web:
    image: igcse-app:latest
    container_name: igcse-web
    ports:
      - "7211:7211"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:7211
      ConnectionStrings__DbConnection: "${DB_CONNECTION_STRING}"
      JWT__Issuer: "${JWT__Issuer}"
      JWT__Audience: "${JWT__Audience}"
      JWT__SigningKey: "${JWT__SigningKey}"
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:7211/api/ping"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
EOF

                    # Deploy with compose
                    docker-compose down || true
                    docker-compose up -d
                    
                    echo "✅ Docker Compose deployment completed"
                    docker-compose ps
                '''
            }
        }

        /* =====================================
           📊 POST-DEPLOYMENT VERIFICATION
        ====================================== */
        stage('Post-Deployment Verification') {
            steps {
                sh '''
                    echo "=== POST-DEPLOYMENT VERIFICATION ==="
                    
                    # Wait for app to be ready
                    sleep 15
                    
                    # Health check
                    echo "Checking app health..."
                    HEALTH_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:7211/api/ping || echo "000")
                    
                    if [ "$HEALTH_RESPONSE" = "200" ]; then
                        echo "✅ Health check passed"
                    else
                        echo "❌ Health check failed (HTTP $HEALTH_RESPONSE)"
                        docker logs igcse-app --tail 50
                        exit 1
                    fi
                    
                    # API test
                    echo "Testing API endpoints..."
                    PING_RESPONSE=$(curl -s http://localhost:7211/api/ping)
                    echo "Ping response: $PING_RESPONSE"
                    
                    if echo "$PING_RESPONSE" | grep -q "pong"; then
                        echo "✅ API test passed"
                    else
                        echo "❌ API test failed"
                        exit 1
                    fi
                    
                    # Show running containers
                    echo "=== RUNNING CONTAINERS ==="
                    docker ps | grep igcse
                    
                    echo "✅ POST-DEPLOYMENT VERIFICATION COMPLETED!"
                '''
            }
        }

        /* =====================================
           🧹 CLEANUP OLD IMAGES
        ====================================== */
        stage('Cleanup Old Images') {
            steps {
                sh '''
                    echo "=== CLEANING UP OLD IMAGES ==="
                    
                    # Remove dangling images
                    docker image prune -f
                    
                    # Keep only last 3 versions
                    docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.CreatedAt}}" | grep igcse-app | tail -n +4 | awk '{print $1":"$2}' | xargs -r docker rmi || true
                    
                    echo "✅ Cleanup completed"
                    docker images | grep igcse-app
                '''
            }
        }
    }

    post {
        always {
            echo '============================================'
            echo '🏁 BUILD PROCESS COMPLETED'
            echo '============================================'
        }
        success {
            echo '============================================'
            echo '✅ BUILD SUCCESS!'
            echo '📁 Application ready in ./deploy/ folder'
            echo '🐳 Docker image: igcse-app:latest'
            echo '🌐 App running on: http://localhost:7211'
            echo '============================================'
        }
        failure {
            echo '============================================'
            echo '❌ BUILD FAILED!'
            echo '📋 Check logs above for details'
            echo '============================================'
        }
    }
}
