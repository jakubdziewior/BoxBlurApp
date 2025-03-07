;------------------------------------------------------------------------------
; Procedure: CalculatePixelSumsASM
; Description: The procedure calculates the sums of RGB components of pixels 
;              in the blur area around a given point (x, y). Used for implementing 
;              an image blur filter.
;
; Input parameters:
;   rcx - pointer to the image buffer (RGB format, 3 bytes/pixel)
;   rdx - image width in pixels
;   r8  - image height in pixels
;   r9  - blur area size (blur size)
;   [rbp+48] - stride (line width in bytes)
;   [rbp+56] - x (x-coordinate of the central point)
;   [rbp+64] - y (y-coordinate of the central point)
;   [rbp+72] - maxIndex (maximum index in the buffer)
;
; Output parameters:
;   [rbp+80]  - pointer to sumR (sum of R component)
;   [rbp+88]  - pointer to sumG (sum of G component)
;   [rbp+96]  - pointer to sumB (sum of B component)
;   [rbp+104] - pointer to count (number of included pixels)
;
; Modified registers: rax, rbx, rcx, rdx, r8-r15, rsi, rdi
;------------------------------------------------------------------------------

.code
CalculatePixelSumsASM proc
    ; Procedure prologue - saving non-volatile registers on the stack
    push rbp                    ; Save frame pointer
    mov rbp, rsp                ; Create a new stack frame
    push rbx                    ; Save non-volatile registers
    push rsi                    ; that will be used in the procedure
    push rdi
    push r12
    push r13
    push r14
    push r15

    ; Initialize vector registers and counters
    vxorps xmm0, xmm0, xmm0     ; Zero out RGB sum register
    xor r15, r15                ; Zero out pixel counter
    
    ; Retrieve parameters from the stack
    mov rsi, [rbp+56]           ; x-coordinate of the central point
    mov rdi, [rbp+64]           ; y-coordinate of the central point
    mov r10, r9                 ; Copy blur size
    neg r10                     ; Negate for starting from -blurSize

blur_y_loop:                    ; Loop over y-coordinate of the blur area
    mov rax, rdi                ; Retrieve y-coordinate
    add rax, r10                ; Calculate y + dy
    cmp rax, 0                  ; Check if it exceeds upper boundary
    jl next_y
    cmp rax, r8                 ; Check if it exceeds lower boundary
    jge next_y
    
    push r10                    ; Save y counter
    mov r10, r9                 ; Prepare x counter
    neg r10                     ; Start from -blurSize

blur_x_loop:                    ; Loop over x-coordinate of the blur area
    mov rbx, rsi                ; Retrieve x-coordinate
    add rbx, r10                ; Calculate x + dx
    cmp rbx, 0                  ; Check left boundary
    jl next_x
    cmp rbx, rdx                ; Check right boundary
    jge next_x
    
    ; Calculate pixel offset in the buffer
    push rax                    ; Save y-coordinate
    mov r11, rax                ; Calculate row offset
    imul r11, [rbp+48]          ; Multiply by stride
    lea rax, [rbx*2 + rbx]      ; Calculate x offset (x * 3 for RGB)
    add r11, rax                ; Total offset = y*stride + x*3
    pop rax
    
    ; Verify buffer boundaries
    cmp r11, 0                  ; Check lower buffer boundary
    jl next_x
    mov rbx, [rbp+72]           ; Retrieve maximum index value
    sub rbx, 3                  ; Adjust for pixel size (RGB)
    cmp r11, rbx                ; Check upper buffer boundary
    jae next_x

    ; Vectorized load and addition of RGB components
    movzx ebx, byte ptr [rcx+r11]      ; Load B component 
    vpinsrb xmm1, xmm1, ebx, 0         ; Insert B into vector 
    movzx ebx, byte ptr [rcx+r11+1]    ; Load G component 
    vpinsrb xmm1, xmm1, ebx, 1         ; Insert G into vector 
    movzx ebx, byte ptr [rcx+r11+2]    ; Load R component 
    vpinsrb xmm1, xmm1, ebx, 2         ; Insert R into vector 
    
    ; Conversion and accumulation 
    vpmovzxbd xmm1, xmm1               ; Convert bytes to DWORDs 
    vpaddd xmm0, xmm0, xmm1            ; Add to sums 
    
    inc r15                            ; Increment pixel counter

next_x:                       ; End of x loop handling 
    inc r10                   ; Next x coordinate 
    cmp r10, r9               ; Check end of x range 
    jle blur_x_loop
    pop r10                   ; Restore y counter 

next_y:                       ; End of y loop handling 
    inc r10                   ; Next y coordinate 
    cmp r10, r9               ; Check end of y range 
    jle blur_y_loop 

    ; Extract results from vector register  
    vextracti128 xmm1, ymm0, 0         ; Extract lower vector part 
    vpextrd r12d, xmm1, 2              ; Extract sum of R 
    vpextrd r13d, xmm1, 1              ; Extract sum of G 
    vpextrd r14d, xmm1, 0              ; Extract sum of B 

    ; Save results to memory
    mov rax, [rbp+80]         ; Save sum of R 
    mov [rax], r12
    mov rax, [rbp+88]         ; Save sum of G 
    mov [rax], r13
    mov rax, [rbp+96]         ; Save sum of B 
    mov [rax], r14
    mov rax, [rbp+104]        ; Save pixel counter 
    mov [rax], r15

    ; Procedure epilogue - restore registers
    pop r15
    pop r14
    pop r13
    pop r12
    pop rdi
    pop rsi
    pop rbx
    pop rbp
    ret
CalculatePixelSumsASM endp
end
