# Initialize repository and README.md
echo "# WeatherDesk" >> README.md
git init
git add README.md
git commit -m "first commit"

# Setup Git LFS for handling large files
git lfs install

# Add a rule to track large files (e.g., files over 100MB)
Get-ChildItem -Recurse -File | Where-Object { $_.Length -gt 100MB } | ForEach-Object {
    git lfs track $_.FullName
}

# Add tracked files and update Git LFS config
git add .gitattributes
git add .
git commit -m "Track large files and add initial content"

# Configure branch and remote
git branch -M main
git remote add origin https://github.com/gamertoky1188gro/WeatherDesk.git

# Push with LFS support
git push -u origin main
