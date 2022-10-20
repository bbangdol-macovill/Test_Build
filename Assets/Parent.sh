#!/bin/sh

# 스크립트명
# echo $0

# 스크립트에 전달된 인자 갯수. (나중에 분명 중요)
# echo $1

# 스크립트의 PID?
# echo $$

# ls 활용
#count=0
#plus=1
#string="./ "
sourcePath="$(pwd)""/"
targetPath="$sourcePath""../TestCopy"
exclude="Shell"

#echo $targetPath

# echo $(pwd)
# echo $targetPath

#for f in $(ls -R); do
#    if [ -d ${f} ];
#        then :
#    else
#        if [ "${f}" == *string* ];
#            then :
#        else
#            # count=$(($count + $plus))
#            # echo "$f"
#            cp -R "$sourcePath""$f" "$targetPath"
#        fi
#    fi
#
## echo $count
#done

# find 활용
#arrayFindResult=$(find . -not -path '*/.*')
#
#for f in $arrayFindResult; do
#    echo "$f"
#    #cp -R "$f" "$targetPath"
#done

# cp 만.
cp -R `ls | grep -v '^Shell'` $targetPath

echo "Test"
