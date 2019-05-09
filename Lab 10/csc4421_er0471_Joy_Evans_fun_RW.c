#include <stdio.h>
#include <unistd.h>
#include <string.h>
#include <fcntl.h>

int main(int argc, char *argv[])
{
    int const BUF_SIZE=10;
    int const EXTRA_SIZE=5;
    int inputFd, outputFd, openFlags;
    mode_t filePerms;
    ssize_t numRead;
    char buf[BUF_SIZE];
    char buf1[EXTRA_SIZE];
    char* inputFileName="input.txt";
    char* outputFileName="output.txt";
    
    inputFd = open(inputFileName, O_RDONLY);
    if (inputFd == -1)
        printf("opening file %s", inputFileName);
    
    openFlags = O_CREAT | O_WRONLY | O_TRUNC;
    filePerms = S_IRUSR | S_IWUSR | S_IRGRP | S_IWGRP |
    S_IROTH | S_IWOTH;      /* rw-rw-rw- */
    outputFd = open(outputFileName, openFlags, filePerms);
    if (outputFd == -1)
        printf("opening file %s", outputFileName);
    
    while ((numRead = read(inputFd, buf, BUF_SIZE)) > 0)
    {
        if (write(outputFd, buf, numRead) != numRead)
            printf("couldn't write whole buffer");
        if (write(outputFd, "EXTRA", EXTRA_SIZE) != EXTRA_SIZE)
            printf("couldn't write whole EXTRA buffer");
    }

    if (numRead == -1)
        printf("read");
    
    if (close(inputFd) == -1)
        printf("close input");
    if (close(outputFd) == -1)
        printf("close output");
   
    return(0);
}
