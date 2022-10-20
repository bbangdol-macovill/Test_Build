@echo off

rem git add
git add ../Assets/AddressableAssetsData/*

echo ADD
pause > nul

rem git commit
git commit -m JenkinsBuild(develop)

echo Commit
pause > nul

rem git push
git push origin develop

echo Push
pause > nul