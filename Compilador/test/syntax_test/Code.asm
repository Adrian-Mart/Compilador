
section .data
    zero dd 0.0
    format db '%f', 10, 0
    string0 db "Quack!", 10
    len0 equ $-string0
    string1 db "Valor de quack_0:", 10
    len1 equ $-string1


section .bss
    a: resd 4
    b: resd 4
    v2: resd 4
    v1: resd 4
    v0: resd 4
    real0: resd 4
    real1: resd 4


section .text
    global main
    extern printf

main:
    ; assign data
    mov dword [real0], __float32__(-1.0)
    

    ; assign data
    mov dword [real1], __float32__(1.0)
    


    ; move value to eax
    ; store data
    mov eax, dword [real0]
    mov dword [a], eax
    mov dword [b], __float32__(2.0)
    
    ; operate
    fld dword [a]
    fadd dword [b]
    fstp dword [v0]
    
    mov eax, dword [v0]

    ; assign data
    mov dword [real0], eax
    call if_name1


    ; exit
    xor   eax, eax
    ret


evalEqual:
    fcomip st1
    fstp st0
    je true
    jne false

evalNotEqual:
    fcomip st1
    fstp st0
    jne true
    je false

evalLess:
    fcomip st1
    fstp st0
    jb true
    jae false

evalGreater:
    fcomip st1
    fstp st0
    ja true
    jbe false

evalAnd:
    fcom dword [zero]
    fstsw ax
    sahf
    jz false

    fxch st1
    fcom dword [zero]
    fstsw ax
    sahf
    jz false

    jmp true

evalOr:
    fcom dword [zero]
    fstsw ax
    sahf
    jnz true

    fxch st1
    fcom dword [zero]
    fstsw ax
    sahf
    jnz true
    
    jmp false

true:
    mov ecx, 1
    ret

false:
    mov ecx, 0
    ret

else:
    ret

print:
    mov eax, 4
    mov ebx, 1
    int 0x80
    ret

print_val:
    sub   esp, 8
    fstp  qword [esp]
    push  format
    call  printf
    add   esp, 12
    extern fflush
    push  0
    call  fflush
    add   esp, 4
    ret
    

while_name0:
    ; evaluate
    ; store data
    mov eax, dword [real0]
    mov dword [a], eax
    mov dword [b], __float32__(0.0)
    
    ; compare
    fld dword [b]
    fld dword [a]
    call evalLess 
    
    cmp ecx, 1
    jne else

    ; codigo
    ; move value to eax
    ; store data
    mov eax, dword [real0]
    mov dword [a], eax
    mov dword [b], __float32__(0.5)
    
    ; operate
    fld dword [a]
    fadd dword [b]
    fstp dword [v2]
    
    mov eax, dword [v2]

    ; assign data
    mov dword [real0], eax
    
    ; print
    mov ecx, string1
    mov edx, len1
    call print
    
    ; print
    fld dword [real0]
    call print_val
    
    jmp while_name0
    

if_name1:
    ; evaluate
    ; store data
    mov eax, dword [real0]
    mov dword [a], eax
    mov eax, dword [real1]
    mov dword [b], eax
    
    ; compare
    fld dword [b]
    fld dword [a]
    call evalEqual 
    
    cmp ecx, 1
    jne else

    ; codigo
    ; print
    mov ecx, string0
    mov edx, len0
    call print
    
    ; assign data
    mov dword [real0], __float32__(-2.0)
    call while_name0

    ret
    

    