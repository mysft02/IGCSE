pipeline {
    agent any
    
    environment {
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "1"
        PATH = "$HOME/.dotnet:${env.PATH}"
        DOTNET_CLI_TELEMETRY_OPTOUT = "1"
        LIQUIBASE_HOME = "$WORKSPACE/.liquibase"
        DB_CONNECTION_STRING = "server=163.223.210.80;port=3306;database=IGCSE;user=root;password=rootpassword;TreatTinyAsBoolean=true;Allow User Variables=true;SslMode=None;AllowPublicKeyRetrieval=True"
        ConnectionStrings__DbConnection = "server=163.223.210.80;port=3306;database=IGCSE;user=root;password=rootpassword;TreatTinyAsBoolean=true;Allow User Variables=true;SslMode=None;AllowPublicKeyRetrieval=True"
    }
    
    stages {
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
        
        stage('Install .NET SDK') {
    steps {
        sh '''
            echo "=== INSTALLING .NET SDK ==="
            curl -L https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
            chmod +x dotnet-install.sh

            # Cài SDK .NET 8 đầy đủ (bao gồm hostfxr)
            ./dotnet-install.sh --channel 8.0 --install-dir $HOME/.dotnet

            export DOTNET_ROOT=$HOME/.dotnet
            export PATH=$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH

            echo "=== VERIFY .NET INSTALL ==="
            dotnet --info
            dotnet --list-runtimes
            dotnet --list-sdks

            # Cài EF CLI (dùng riêng trong pipeline)
            dotnet tool install --tool-path "$DOTNET_ROOT/tools" dotnet-ef || true
            "$DOTNET_ROOT/tools/dotnet-ef" --version
        '''
    }
}


        stage('Run EF Migrations') {
    steps {
        sh '''
        echo "=== INSTALL DEPENDENCIES ==="
            if [ -f /etc/alpine-release ]; then
                apk add --no-cache bash icu-libs curl
            else
                if command -v apt-get >/dev/null 2>&1 && [ "$(id -u)" -eq 0 ]; then
                    apt-get update && apt-get install -y curl
                else
                    echo "Skipping apt-get (not root or apt-get unavailable); assuming curl exists."
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



        stage('Run Liquibase') {
            steps {
                sh '''
                    echo "=== RUNNING LIQUIBASE MIGRATIONS ==="

                    # Cài unzip nếu thiếu
                    if [ -f /etc/alpine-release ]; then
                        apk add --no-cache unzip curl || true
                    else
                        if command -v apt-get >/dev/null 2>&1 && [ "$(id -u)" -eq 0 ]; then
                            apt-get update && apt-get install -y unzip curl || true
                        else
                            echo "Skipping apt-get (not root or apt-get unavailable); assuming unzip/curl exist."
                        fi
                    fi

                    # Cài Liquibase vào workspace nếu chưa có
                    if [ ! -f "$LIQUIBASE_HOME/liquibase" ]; then
                        echo "Liquibase not found. Installing to $LIQUIBASE_HOME ..."
                        mkdir -p "$LIQUIBASE_HOME"
                        curl -L https://github.com/liquibase/liquibase/releases/download/v4.29.2/liquibase-4.29.2.zip -o /tmp/liquibase.zip
                        unzip -o /tmp/liquibase.zip -d "$LIQUIBASE_HOME" >/dev/null 2>&1 || unzip -o /tmp/liquibase.zip -d "$LIQUIBASE_HOME"
                        # Một số bản phát hành có thư mục con, di chuyển nếu cần
                        if [ ! -f "$LIQUIBASE_HOME/liquibase" ]; then
                            inner=$(find "$LIQUIBASE_HOME" -maxdepth 1 -type d -name "liquibase-*" | head -1)
                            if [ -n "$inner" ]; then
                                mv "$inner"/* "$LIQUIBASE_HOME"/
                            fi
                        fi
                        chmod +x "$LIQUIBASE_HOME/liquibase" || true
                    fi

                    if [ ! -f "$LIQUIBASE_HOME/liquibase" ]; then
                        echo "❌ Liquibase still not found at $LIQUIBASE_HOME"
                        ls -la "$LIQUIBASE_HOME" || true
                        exit 1
                    fi

                    # Chạy lệnh liquibase update tại thư mục Migration
                    cd Migration
                    echo "Using defaultsFile=$(pwd)/liquibase.properties"
                    $LIQUIBASE_HOME/liquibase \
                        --defaultsFile=liquibase.properties \
                        update

                    echo "✅ Liquibase migrations applied successfully."
                '''
            }
        }
        
        stage('Build Application') {
            steps {
                sh '''
                    echo "=== BUILD PROCESS ==="
                    
                    # Tìm file solution (trong thư mục con nếu có)
                    SOLUTION_FILE=$(find . -name "IGCSE.sln" | head -1)

                    if [ -n "$SOLUTION_FILE" ]; then
                        echo "✅ Found solution file: $SOLUTION_FILE"
                        dotnet restore "$SOLUTION_FILE"
                        dotnet build "$SOLUTION_FILE" --configuration Release

                        # Publish project chính (IGCSE.csproj)
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

        stage('Smoke Test') {
            steps {
                sh '''
                    echo "=== RUNNING SMOKE TEST ==="

                    # Ưu tiên lấy file chính (IGCSE.dll)
                    DLL_FILE=$(find ./publish -maxdepth 1 -name "IGCSE.dll" | head -1)

                    if [ -z "$DLL_FILE" ]; then
                        echo "❌ ERROR: Main DLL (IGCSE.dll) not found in publish folder!"
                        ls -la ./publish/
                        exit 1
                    fi

                    echo "✅ Found DLL file: $DLL_FILE"

                    export ASPNETCORE_URLS="http://0.0.0.0:7211"
                    nohup dotnet "$DLL_FILE" > smoke_test.log 2>&1 &
                    APP_PID=$!

                    sleep 5

                    if ps -p $APP_PID > /dev/null; then
                        echo "✅ App started successfully (PID: $APP_PID)"
                        kill $APP_PID
                    else
                        echo "❌ App failed to start"
                        echo "=== SMOKE TEST LOG ==="
                        cat smoke_test.log
                        exit 1
                    fi
                '''
            }
            post {
                success {
                    echo '✅ Smoke test passed!'
                }
                failure {
                    echo '❌ Smoke test failed!'
                }
            }
        }

        stage('Run App') {
            steps {
                sh '''
                    pkill -f "dotnet ./publish/IGCSE.dll" || true
                    export ASPNETCORE_URLS="http://0.0.0.0:7211"
                    nohup dotnet ./publish/IGCSE.dll > app.out 2>&1 &
                    sleep 3
                    echo "=== PROCESS CHECK ==="
                    ps aux | grep "IGCSE.dll" | grep -v grep || true
                    echo "=== LAST LOGS ==="
                    tail -n 200 app.out || true
                '''
            }
        }

        stage('Deploy Local') {
            steps {
                sh '''
                    echo "=== DEPLOYING LOCALLY ==="
                    ls -la ./publish/
                    mkdir -p ./deploy
                    cp -r ./publish/* ./deploy/
                    chmod -R 755 ./deploy
                    echo "=== DEPLOYMENT COMPLETED ==="
                    ls -la ./deploy/
                    echo ""
                    echo "✅ LOCAL DEPLOYMENT COMPLETED!"
                    echo "📁 Files are ready in: ./deploy/"
                '''
            }
        }
    }
    
    post {
        always {
            echo 'Build process completed'
        }
        success {
            echo '✅ Build SUCCESS!'
            echo '📁 Application ready in ./deploy/ folder'
        }
        failure {
            echo '❌ Build FAILED!'
        }
    }
}
