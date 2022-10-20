#!/bin/sh

sourcePath="$(pwd)""/"
targetPath="$sourcePath""../TestCopy"

# ls.
#count=0
#plus=1
#string="./ "
#exclude="Shell"
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

# find.
#arrayFindResult=$(find . -not -path '*/.*')
#
#for f in $arrayFindResult; do
#    echo "$f"
#    #cp -R "$f" "$targetPath"
#done

# cp.
cp -R `ls | grep -v '^Shell'` $targetPath
echo "Build Version : "$1
