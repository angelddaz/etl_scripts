[string]$archiveRoot = "C:\FileLocation\" 
[string]$FileFilter = "filenames*.zip" 
[int]$DaysToKeep = 45



if ($DaysToKeep -gt 0)
{$DaysToKeep *= -1}

#this sets the threshold limit of what to keep
$limit = (Get-Date).AddDays($DaysToKeep);

foreach($file in Get-ChildItem -Path $archiveRoot -Recurse -Force -Filter $FileFilter)
{
    #if file is newer than limit, skip it
    if($file.CreationTime -gt $limit)
    {
        continue;
    }

#   Write-Output("Deleting file: " + $file.FullName);
# Enable line below when you are ready to have script delete files (after testing)
                    remove-item $file.FullName;   
}
# This line can be disable after you are satisfied with testing results
# Read-Host - Prompt "Press Enter to Exit"
