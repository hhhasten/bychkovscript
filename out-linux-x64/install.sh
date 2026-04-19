#!/bin/bash
echo "Installing porn, malwares and BychkovScript."

echo "Creating BychkovScript directory..."
sudo mkdir -p /usr/local/lib/bychkovscript

echo "Copying binary file to BychkovScript directory..."
sudo cp BychkovScript.CLI /usr/local/lib/bychkovscript/bs

echo "Copying standart library to BychkovScript directory..."
sudo cp -r stdlib /usr/local/lib/bychkovscript/

echo "Making file executable..."
sudo chmod +x /usr/local/lib/bychkovscript/bs

echo "Adding BychkovScript to PATH..."
sudo ln -sf /usr/local/lib/bychkovscript/bs /usr/local/bin/bs

echo "Your computer is finally infected by BychkovScript, enter 'bs --version' to check"