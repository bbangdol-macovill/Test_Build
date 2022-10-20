#!/bin/sh

# 스크립트명
echo $0

# 스크립트에 전달된 인자 갯수. (나중에 분명 중요)
# echo $1

# 스크립트의 PID?
# echo $$

for f in $(ls -r); do
    if [ -f ${f} ];
        then echo "$f"
    else echo "Folder"
    fi
done
