
section .data
    format: db "( ( 2.3 / 1.2 ) * 0.5 ) - 34 = %5f",10,0

section .bss
    a: resd 4
    b: resd 4
    v0: resd 4

    
section .text
    global main
    extern printf

main:

    mov dword [a], __float32__(2.300000)
    mov dword [b], __float32__(1.200000)
    
    fld dword [a]
    fdiv dword [b]
    fstp dword [v0]
    
    mov eax, dword [v0]
    mov dword [a], eax
    mov dword [b], __float32__(0.500000)
    
    fld dword [a]
    fmul dword [b]
    fstp dword [v0]
    
    mov eax, dword [v0]
    mov dword [a], eax
    mov dword [b], __float32__(34.000000)
    
    fld dword [a]
    fsub dword [b]
    fstp dword [v0]
    
    
    ; print result
    fld dword [v0]
    sub   esp, 8
    fstp  qword [esp]
    push  format
    call  printf
    add   esp, 12
    
    ; exit
    xor   eax, eax
    ret
    