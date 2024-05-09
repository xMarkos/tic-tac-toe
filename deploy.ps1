# Build the app
dotnet publish -c Release -o out\bin

# Create package for zip publish
Compress-Archive -Path out\bin\* -DestinationPath out\tic-tac-toe.zip -Force

# Deploy the zip file
az webapp deploy --resource-group InterviewDemos --name yet-another-tic-tac-toe --src-path out\tic-tac-toe.zip --type zip
