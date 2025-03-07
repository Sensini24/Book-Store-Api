# Cambios en proyecto
1. Eliminé BookId de Order porque una orden puede almacenar muchos libros.
2. Agregué dos entidades WishList y WishListDetails. El primero esta asociado a User con su UserId.El segundo surge de la relación de muchos a muchos entre Book y WishList.