#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <fcntl.h>
#include <unistd.h>

#define BUF_SIZE 500

int main(int argc, char *argv[])
{ 

int in_fd;
int rd_count;
char buffer[BUF_SIZE];



in_fd=open(argv[1],O_RDONLY);

//if file can't be opened/does not exist exit
if(in_fd<0)
{
printf("file can't be opened");
exit(1);
}


while(read(in_fd,buffer,sizeof(buffer))!=0)
{

    printf("%s",buffer);
    if(rd_count<=0)
    {
        break;
        exit(2);
    }
}
  
close(in_fd);  
exit (0);

    


}