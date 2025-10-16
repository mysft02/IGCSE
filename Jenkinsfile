pipeline {
    agent any
    
    environment {
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "1"
        PATH = "$HOME/.dotnet:${env.PATH}"
        DOTNET_CLI_TELEMETRY_OPTOUT = "1"
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
                    ./dotnet-install.sh --channel 8.0
                    echo "=== .NET VERSION ==="
                    dotnet --version
                '''
            }
        }
        
        stage('Build Application') {
            steps {
                sh '''
                    echo "=== BUILD PROCESS ==="
                    
                    # Tìm file project/solution
                    if [ -f "*.sln" ]; then
                        echo "Building solution file"
                        dotnet restore
                        dotnet build --configuration Release
                        dotnet publish "$PROJECT_FILE" -c Release -o ./publish
                    elif [ -n "$(find . -name '*.csproj' | head -1)" ]; then
                        echo "Building project file"
                        PROJECT_FILE=$(find . -name "*.csproj" | head -1)
                        echo "Using project file: $PROJECT_FILE"
                        dotnet restore "$PROJECT_FILE"
                        dotnet build "$PROJECT_FILE" --configuration Release
                        dotnet publish "$PROJECT_FILE" -c Release -o ./publish
                    else
                        echo "ERROR: No project or solution files found!"
                        echo "Current directory contents:"
                        ls -la
                        exit 1
                    fi
                    
                    echo "=== PUBLISHED OUTPUT ==="
                    ls -la ./publish/
                '''
            }
        }

        stage('Run App') {
    steps {
        sh '''
            # Dừng tiến trình cũ (nếu có)
            pkill -f "dotnet ./publish/IGCSE.dll" || true

            # Chạy app ở 0.0.0.0:5121 (không cần quyền root)
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
                    
                    # Kiểm tra thư mục publish
                    echo "=== CHECKING PUBLISH FILES ==="
                    ls -la ./publish/
                    
                    # Tạo thư mục deploy trong workspace (không cần sudo)
                    mkdir -p ./deploy
                    
                    # Copy files đến thư mục deploy
                    cp -r ./publish/* ./deploy/
                    
                    # Set permissions (không cần sudo trong workspace)
                    chmod -R 755 ./deploy
                    
                    echo "=== DEPLOYMENT COMPLETED ==="
                    echo "Files deployed to: ./deploy/"
                    echo "Contents:"
                    ls -la ./deploy/
                    
                    # Hiển thị deployment instructions
                    echo ""
                    echo "✅ LOCAL DEPLOYMENT COMPLETED!"
                    echo "📁 Files are ready in: ./deploy/"
                    echo ""
                    echo "NEXT STEPS:"
                    echo "1. Manual copy to production server:"
                    echo "   scp -r ./deploy/* user@server:/var/www/myapp/"
                    echo "2. Or configure automated deployment in next stage"
                '''
            }
        }
        
        stage('Deploy Instructions') {
            steps {
                sh '''
                    echo "=== DEPLOYMENT INSTRUCTIONS ==="
                    echo ""
                    echo "OPTION 1 - Manual Deployment:"
                    echo "   scp -r ./deploy/* ubuntu@163.223.210.80:/var/www/myapp/"
                    echo "   ssh ubuntu@163.223.210.80 'sudo systemctl restart myapp.service'"
                    echo ""
                    echo "OPTION 2 - Add automated SSH deployment stage later"
                    echo ""
                    echo "OPTION 3 - Use Docker deployment"
                    echo "   docker build -t myapp ."
                    echo "   docker run -d -p 8080:80 myapp"
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