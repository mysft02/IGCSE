#!/usr/bin/env node

const { execSync } = require("child_process");
const fs = require("fs");
const path = require("path");
const readline = require("readline");

// VPS Configuration
const VPS_IP = "163.223.210.80";
const VPS_USER = "root";
const VPS_PASSWORD = "DataZ6M6s31eHE8K%41o";
const CONTAINER_NAME_5121 = "igcse-app-5121";
const CONTAINER_NAME_7211 = "igcse-app-7211";
const IMAGE_NAME = "igcse:latest";
const IMAGE_TAR = "igcse-latest.tar";
const PROJECT_NAME = "igcse";
const PORT_5121 = 5121;
const PORT_7211 = 7211;

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout,
});

function printMenu() {
  console.log("\n╔════════════════════════════════════════╗");
  console.log("║     CI/CD Deployment Menu              ║");
  console.log("╚════════════════════════════════════════╝\n");
  console.log("1)  Build Docker image");
  console.log("2)  Save Docker image to tar");
  console.log("3)  Load Docker image on server");
  console.log("4)  Deploy container (Port 5121)");
  console.log("5)  Deploy container (Port 7211)");
  console.log("6)  Deploy both containers (5121 + 7211)");
  console.log("7)  Full deployment (Build + Save + Load + Deploy both)");
  console.log("8)  Show container logs (select port)");
  console.log("9)  Check container status");
  console.log("10) Restart container (select port)");
  console.log("11) Stop container (select port)");
  console.log("12) Remove container (select port)");
  console.log("13) Run DB migrations (Liquibase menu)");
  console.log("14) Execute command in container (select port)");
  console.log("15) View container environment variables (select port)");
  console.log(
    "16) Cleanup Docker on LOCAL (remove unused images, containers, build cache)"
  );
  console.log(
    "17) Cleanup Docker on VPS (remove unused images, containers, build cache on server)"
  );
  console.log("18) Exit\n");
}

function executeCommand(command, description) {
  console.log(`\n${description}...`);
  try {
    execSync(command, { stdio: "inherit" });
    console.log(`✓ ${description} completed successfully\n`);
    return true;
  } catch (error) {
    console.error(`✗ ${description} failed:`, error.message);
    return false;
  }
}

function sshCommand(command) {
  return `sshpass -p '${VPS_PASSWORD}' ssh -o StrictHostKeyChecking=no ${VPS_USER}@${VPS_IP} '${command}'`;
}

function scpCommand(localPath, remotePath) {
  return `sshpass -p '${VPS_PASSWORD}' scp -o StrictHostKeyChecking=no ${localPath} ${VPS_USER}@${VPS_IP}:${remotePath}`;
}

function buildDockerImage() {
  console.log("\nBuilding Docker image...");
  console.log("This may take several minutes. Please wait...\n");
  try {
    execSync(
      `docker build --platform linux/amd64 -t ${IMAGE_NAME} -f IGCSE/Dockerfile . --progress=plain`,
      {
        stdio: "inherit",
      }
    );
    console.log(`✓ Building Docker image completed successfully\n`);
    return true;
  } catch (error) {
    console.error(`✗ Building Docker image failed`);
    console.error(`\nTo see detailed errors, run manually:`);
    console.error(
      `docker build --platform linux/amd64 -t ${IMAGE_NAME} -f IGCSE/Dockerfile . --progress=plain\n`
    );
    return false;
  }
}

function saveDockerImage() {
  return executeCommand(
    `docker save ${IMAGE_NAME} -o ${IMAGE_TAR}`,
    "Saving Docker image to tar"
  );
}

function loadDockerImageOnServer() {
  const remotePath = `~/${IMAGE_TAR}`;
  console.log("\nUploading Docker image to server...");
  if (
    !executeCommand(
      scpCommand(IMAGE_TAR, remotePath),
      "Uploading image to server"
    )
  ) {
    return false;
  }

  return executeCommand(
    sshCommand(`docker load -i ${remotePath} && rm ${remotePath}`),
    "Loading Docker image on server"
  );
}

function deployContainer(port, containerName) {
  // First, ensure directories and files exist on server
  // Use a single line command to avoid quote issues
  const setupCmd = `mkdir -p /var/www/igcse/wwwroot && if [ -d /var/www/igcse/appsettings.json ]; then rm -rf /var/www/igcse/appsettings.json; fi && if [ ! -f /var/www/igcse/appsettings.json ]; then echo "{}" > /var/www/igcse/appsettings.json && echo "Created /var/www/igcse/appsettings.json"; fi`;

  console.log(
    `\nSetting up directories and files on server for port ${port}...`
  );
  if (!executeCommand(sshCommand(setupCmd), `Setting up server directories`)) {
    return false;
  }

  const dockerComposeCmd = `
    docker stop ${containerName} 2>/dev/null || true;
    docker rm ${containerName} 2>/dev/null || true;
    docker run -d \\
      --name ${containerName} \\
      --restart unless-stopped \\
      --platform linux/amd64 \\
      -p ${port}:8081 \\
      -v /var/www/igcse/wwwroot:/app/wwwroot \\
      -v /var/www/igcse/appsettings.json:/app/appsettings.json \\
      ${IMAGE_NAME}
  `;

  return executeCommand(
    sshCommand(dockerComposeCmd),
    `Deploying container on port ${port}`
  );
}

function deployContainer5121() {
  return deployContainer(PORT_5121, CONTAINER_NAME_5121);
}

function deployContainer7211() {
  return deployContainer(PORT_7211, CONTAINER_NAME_7211);
}

function deployBothContainers() {
  console.log("\n🚀 Deploying both containers...\n");
  const success1 = deployContainer5121();
  const success2 = deployContainer7211();
  if (success1 && success2) {
    console.log("\n✅ Both containers deployed successfully!\n");
  }
  return success1 && success2;
}

async function showContainerLogs() {
  const port = await askQuestion("Select port (5121 or 7211): ");
  const containerName =
    port === "5121"
      ? CONTAINER_NAME_5121
      : port === "7211"
      ? CONTAINER_NAME_7211
      : null;
  if (!containerName) {
    console.log("Invalid port. Please select 5121 or 7211.");
    return false;
  }
  return executeCommand(
    sshCommand(`docker logs -f ${containerName}`),
    `Showing container logs (port ${port})`
  );
}

function checkContainerStatus() {
  return executeCommand(
    sshCommand(
      `docker ps -a | grep -E '${CONTAINER_NAME_5121}|${CONTAINER_NAME_7211}' || echo "No containers found"`
    ),
    "Checking container status"
  );
}

async function restartContainer() {
  const port = await askQuestion("Select port (5121 or 7211): ");
  const containerName =
    port === "5121"
      ? CONTAINER_NAME_5121
      : port === "7211"
      ? CONTAINER_NAME_7211
      : null;
  if (!containerName) {
    console.log("Invalid port. Please select 5121 or 7211.");
    return false;
  }
  return executeCommand(
    sshCommand(`docker restart ${containerName}`),
    `Restarting container (port ${port})`
  );
}

async function stopContainer() {
  const port = await askQuestion("Select port (5121 or 7211): ");
  const containerName =
    port === "5121"
      ? CONTAINER_NAME_5121
      : port === "7211"
      ? CONTAINER_NAME_7211
      : null;
  if (!containerName) {
    console.log("Invalid port. Please select 5121 or 7211.");
    return false;
  }
  return executeCommand(
    sshCommand(`docker stop ${containerName}`),
    `Stopping container (port ${port})`
  );
}

async function removeContainer() {
  const port = await askQuestion("Select port (5121 or 7211): ");
  const containerName =
    port === "5121"
      ? CONTAINER_NAME_5121
      : port === "7211"
      ? CONTAINER_NAME_7211
      : null;
  if (!containerName) {
    console.log("Invalid port. Please select 5121 or 7211.");
    return false;
  }
  return executeCommand(
    sshCommand(
      `docker stop ${containerName} 2>/dev/null; docker rm ${containerName}`
    ),
    `Removing container (port ${port})`
  );
}

function runLiquibaseMigrations() {
  console.log("\nOpening Liquibase menu...");
  return executeCommand(
    sshCommand(`cd /var/www/igcse/Migration && bash liquibase-menu.sh`),
    "Running Liquibase migrations"
  );
}

async function executeCommandInContainer() {
  const port = await askQuestion("Select port (5121 or 7211): ");
  const containerName =
    port === "5121"
      ? CONTAINER_NAME_5121
      : port === "7211"
      ? CONTAINER_NAME_7211
      : null;
  if (!containerName) {
    console.log("Invalid port. Please select 5121 or 7211.");
    return false;
  }
  const command = await askQuestion("Enter command to execute in container: ");
  if (command.trim()) {
    return executeCommand(
      sshCommand(`docker exec -it ${containerName} ${command}`),
      `Executing command in container (port ${port})`
    );
  }
}

async function viewContainerEnv() {
  const port = await askQuestion("Select port (5121 or 7211): ");
  const containerName =
    port === "5121"
      ? CONTAINER_NAME_5121
      : port === "7211"
      ? CONTAINER_NAME_7211
      : null;
  if (!containerName) {
    console.log("Invalid port. Please select 5121 or 7211.");
    return false;
  }
  return executeCommand(
    sshCommand(`docker exec ${containerName} env | sort`),
    `Viewing container environment variables (port ${port})`
  );
}

function cleanupDockerLocal() {
  return executeCommand(
    `docker system prune -af --volumes`,
    "Cleaning up Docker on LOCAL"
  );
}

function cleanupDockerVPS() {
  return executeCommand(
    sshCommand(`docker system prune -af --volumes`),
    "Cleaning up Docker on VPS"
  );
}

function fullDeployment() {
  console.log("\n🚀 Starting full deployment (both ports)...\n");

  if (!buildDockerImage()) return false;
  if (!saveDockerImage()) return false;
  if (!loadDockerImageOnServer()) return false;
  if (!deployBothContainers()) return false;

  console.log("\n✅ Full deployment completed successfully!\n");
  return true;
}

function askQuestion(question) {
  return new Promise((resolve) => {
    rl.question(question, (answer) => {
      resolve(answer);
    });
  });
}

async function main() {
  console.log("Welcome to IGCSE CI/CD Deployment Tool\n");

  // Check if required tools are installed
  try {
    execSync("which docker", { stdio: "ignore" });
    execSync("which sshpass", { stdio: "ignore" });
  } catch (error) {
    console.error("Error: Please install docker and sshpass first.");
    console.error(
      "Install sshpass: brew install hudochenkov/sshpass/sshpass (macOS)"
    );
    process.exit(1);
  }

  while (true) {
    printMenu();
    const choice = await askQuestion("Select an option (1-18): ");

    switch (choice.trim()) {
      case "1":
        buildDockerImage();
        break;
      case "2":
        saveDockerImage();
        break;
      case "3":
        loadDockerImageOnServer();
        break;
      case "4":
        deployContainer5121();
        break;
      case "5":
        deployContainer7211();
        break;
      case "6":
        deployBothContainers();
        break;
      case "7":
        fullDeployment();
        break;
      case "8":
        await showContainerLogs();
        break;
      case "9":
        checkContainerStatus();
        break;
      case "10":
        await restartContainer();
        break;
      case "11":
        await stopContainer();
        break;
      case "12":
        await removeContainer();
        break;
      case "13":
        runLiquibaseMigrations();
        break;
      case "14":
        await executeCommandInContainer();
        break;
      case "15":
        await viewContainerEnv();
        break;
      case "16":
        const confirmLocal = await askQuestion(
          "Are you sure you want to cleanup Docker on LOCAL? (yes/no): "
        );
        if (confirmLocal.toLowerCase() === "yes") {
          cleanupDockerLocal();
        }
        break;
      case "17":
        const confirmVPS = await askQuestion(
          "Are you sure you want to cleanup Docker on VPS? (yes/no): "
        );
        if (confirmVPS.toLowerCase() === "yes") {
          cleanupDockerVPS();
        }
        break;
      case "18":
        console.log("\nGoodbye! 👋\n");
        rl.close();
        process.exit(0);
      default:
        console.log("\nInvalid option. Please select 1-18.\n");
    }

    if (choice !== "18") {
      await askQuestion("\nPress Enter to continue...");
    }
  }
}

main().catch(console.error);
