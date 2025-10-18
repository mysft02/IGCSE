pipeline {
    agent any

    environment {
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "1"
        PATH = "$HOME/.dotnet:${env.PATH}"
        DOTNET_CLI_TELEMETRY_OPTOUT = "1"
        LIQUIBASE_HOME = "$WORKSPACE/.liquibase"

        DB_CONNECTION_STRING = "server=163.223.210.80;port=3306;database=IGCSE;user=root;password=rootpassword;TreatTinyAsBoolean=true;Allow User Variables=true;SslMode=None;AllowPublicKeyRetrieval=True"
        ConnectionStrings__DbConnection = "server=163.223.210.80;port=3306;database=IGCSE;user=root;password=rootpassword;TreatTinyAsBoolean=true;Allow User Variables=true;SslMode=None;AllowPublicKeyRetrieval=True"

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
