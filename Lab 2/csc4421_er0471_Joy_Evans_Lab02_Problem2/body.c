#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <fcntl.h>
#include <unistd.h>

#define BUF_SIZE 1

int main(int argc, char *argv[])
{ 

int in_fd;
int rd_count;
int head = (int)argv[1];
int tail = (int)argv[2];
char buffer[BUF_SIZE];



in_fd=open(argv[3],O_RDONLY);

//if file can't be opened/does not exist exit
if(in_fd<0)
{
printf("file can't be opened \n");
exit(1);
}

int count=0;
//read file and print
while(read(in_fd,buffer,sizeof(buffer))!=0)
{
if(*buffer =='\n')
{
    count++;
}
if(count>=head && count<=tail)
{
    
    printf("%d: %s",count, buffer);


}
}
  

close(in_fd);  
exit (0);
    


}
