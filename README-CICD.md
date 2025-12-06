# CI/CD Deployment Guide

This project includes a CI/CD deployment system for deploying the IGCSE application to VPS.

## Prerequisites

1. **Node.js** (v14 or higher)
2. **Yarn** package manager
3. **Docker** installed and running
4. **sshpass** for SSH password authentication

### Installing sshpass

**macOS:**

```bash
brew install hudochenkov/sshpass/sshpass
```

**Linux (Ubuntu/Debian):**

```bash
sudo apt-get install sshpass
```

## Setup

1. Install dependencies:

```bash
yarn install
```

Or run the setup script:

```bash
bash scripts/setup.sh
```

## Usage

Run the CI/CD menu:

```bash
yarn ci
```

## Menu Options

1. **Build Docker image** - Builds the Docker image locally
2. **Save Docker image to tar** - Saves the built image to a tar file
3. **Load Docker image on server** - Uploads and loads the image on the VPS
4. **Deploy container (Port 5121)** - Deploys container on port 5121
5. **Deploy container (Port 7211)** - Deploys container on port 7211
6. **Deploy both containers** - Deploys both containers (5121 + 7211)
7. **Full deployment** - Executes steps 1-3 + 6 (Build + Save + Load + Deploy both)
8. **Show container logs** - Displays container logs (select port)
9. **Check container status** - Shows status of both containers
10. **Restart container** - Restarts container (select port)
11. **Stop container** - Stops container (select port)
12. **Remove container** - Removes container (select port)
13. **Run DB migrations** - Opens Liquibase migration menu
14. **Execute command in container** - Runs a custom command (select port)
15. **View container environment variables** - Shows environment variables (select port)
16. **Cleanup Docker on LOCAL** - Cleans up unused Docker resources locally
17. **Cleanup Docker on VPS** - Cleans up unused Docker resources on the server
18. **Exit** - Exits the menu

## VPS Configuration

The deployment is configured for:

- **IP:** 163.223.210.80
- **User:** root
- **Container Names:**
  - `igcse-app-5121` (Port 5121)
  - `igcse-app-7211` (Port 7211)
- **Ports:**
  - 5121 → Container port 8081
  - 7211 → Container port 8081

## Docker Image

- **Image Name:** igcse:latest
- **Tar File:** igcse-latest.tar

## Volume Mounts

The container mounts:

- `/var/www/igcse/wwwroot` → `/app/wwwroot`
- `/var/www/igcse/appsettings.json` → `/app/appsettings.json`

## Quick Deployment

For a full deployment (both ports) in one go:

```bash
yarn ci
# Select option 7 (Full deployment)
```

To deploy only one port:

```bash
yarn ci
# Select option 4 (Port 5121) or option 5 (Port 7211)
```

## Troubleshooting

### Connection Issues

- Verify SSH access: `ssh root@163.223.210.80`
- Check if sshpass is installed correctly
- Verify firewall settings on VPS

### Docker Issues

- Ensure Docker is running: `docker ps`
- Check Docker daemon status
- Verify image was built: `docker images`

### Container Issues

- Check logs: Option 6 in menu
- Verify container status: Option 7 in menu
- Restart container: Option 8 in menu

## Security Notes

⚠️ **Important:** The VPS password is hardcoded in the script. For production use:

- Use SSH keys instead of password
- Store credentials in environment variables
- Use a secrets management system
