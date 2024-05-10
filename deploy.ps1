# Build the app
dotnet publish -c Release -o out\bin

# Load config
$config = cat terraform/config.auto.tfvars.json | convertfrom-json

# Create package for zip publish
Compress-Archive -Path out\bin\* -DestinationPath $config.app_zip_file -Force

# Deploy the zip file
az webapp deploy --resource-group $config.rg_name --name $config.app_name --src-path $config.app_zip_file --type zip
