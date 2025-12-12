#!/bin/bash

# Setup script for CI/CD
# This script installs required dependencies

echo "Setting up CI/CD environment..."

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "Node.js is not installed. Please install Node.js first."
    exit 1
fi

# Check if yarn is installed
if ! command -v yarn &> /dev/null; then
    echo "Installing yarn..."
    npm install -g yarn
fi

# Install dependencies
echo "Installing dependencies..."
yarn install

# Check if docker is installed
if ! command -v docker &> /dev/null; then
    echo "Warning: Docker is not installed. Please install Docker to use CI/CD features."
fi

# Check if sshpass is installed
if ! command -v sshpass &> /dev/null; then
    echo "Warning: sshpass is not installed."
    echo "Install on macOS: brew install hudochenkov/sshpass/sshpass"
    echo "Install on Linux: sudo apt-get install sshpass"
fi

echo "Setup completed!"

