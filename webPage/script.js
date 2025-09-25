let idTokenCopy;

let cart = [];     
let total = 0;
function handleCredentialResponse(response) {
    const idToken = response.credential;
    
    // Decodificar el JWT (payload en base64)
    const payload = JSON.parse(atob(idToken.split('.')[1]));
    console.log("TOKEN : ", idToken);
    console.log('user_name ', payload.name, ' user_picture ', payload.picture, ' user_email ', payload.email);
    // Guardar en localStorage
    localStorage.setItem('id_token', idToken);
    localStorage.setItem('user_name', payload.name);
    localStorage.setItem('user_email', payload.email);
    alert("OK");

    // Redirigir
    window.location.href = 'productos.html';
    
}


function SetData()
{
    const userName = localStorage.getItem('user_name');
    const userEmail = localStorage.getItem('user_email');
    

    if (userName && userEmail ) {
        
        userInfoDiv.innerHTML = `
        <img src="http://static.photos/people/200x200/117" alt="avatar" class="avatar"  width="45" >
        <div>Bienvenido, ${userName}</div>
        `;
    } else {
        
        userInfoDiv.innerHTML = `
        <img src="http://static.photos/people/200x200/" alt="avatar" class="avatar"  width="45" >    
        <h3>Bienvenido, Invitado</h3>
        `;
    }
}
function  SignOut(){
        google.accounts.id.disableAutoSelect(); 
        localStorage.removeItem('id_token');
        localStorage.removeItem('user_name');
        localStorage.removeItem('user_email');
        localStorage.removeItem('user_picture');
        alert("Sesión cerrada");
        window.location.href = 'index.html';
}

function increaseQuantity(id) {
    const quantityElement = document.getElementById(`quantity-${id}`);
    let quantity = parseInt(quantityElement.textContent);
    quantity++;
    quantityElement.textContent = quantity;
}
function decreaseQuantity(id) {
    const quantityElement = document.getElementById(`quantity-${id}`);
    let quantity = parseInt(quantityElement.textContent);
    if (quantity > 1) {
        quantity--;
        quantityElement.textContent = quantity;
    }
}
function addToCart(id, name, price) {
    const quantity = parseInt(document.getElementById(`quantity-${id}`).textContent);
    const item = { id: Number(id), name, price, quantity }; 
    console.log("Adding to cart: ", item);
    const existingItem = cart.find(i => i.id === id);
    if (existingItem) {
        existingItem.quantity += quantity;
    } else {
        cart.push(item);
    }

    updateCart();
    resetQuantity(id);
}
function resetQuantity(id) {
    document.getElementById(`quantity-${id}`).textContent = '1';
}
function updateCart() {
    const cartItemsElement = document.getElementById('cartItems');
    const cartTotalElement = document.getElementById('cartTotal');
    const cartCountElement = document.getElementById('cartCount');
    
    if (cart.length === 0) {
        cartItemsElement.innerHTML = '<p>Your cart is empty</p>';
        cartTotalElement.textContent = '$0';
        cartCountElement.textContent = '0';
        return;
    }
    
    let html = '';
    total = 0;
    let itemCount = 0;
    
    cart.forEach(item => {
        const itemTotal = item.price * item.quantity;
        total += itemTotal;
        itemCount += item.quantity;
        
        html += `
            <div class="cart-item">
                <span>${item.name} x${item.quantity}</span>
                <span>$${itemTotal}</span>
            </div>
        `;
    });
    
    cartItemsElement.innerHTML = html;
    cartTotalElement.textContent = `$${total}`;
    cartCountElement.textContent = itemCount;
}

function prepareOrderPayload() {
    
    const userEmail = localStorage.getItem('user_email');

    // Calcular el total
    const totalAmount = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    
    
 const orderItems = cart.map(item => ({
        Name: item.name,
        ProductId: item.id,
        Quantity: item.quantity,
        UnitPrice: item.price
    }));

    return {
        UserEmail: userEmail,
        Items: orderItems
    };
}

console.log (Items);
function processPayment() {
    placeOrder();
    const orderPayload = prepareOrderPayload();
    const idTokenPayment = localStorage.getItem('id_token');
    console.log("TOKEN AUTHORIZATION: ", idTokenPayment);
    fetch('http://localhost:7256/api/orders', {
        method: 'POST',
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${idTokenPayment}`
            
        },
        body: JSON.stringify(orderPayload)
        
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        alert(`Payment processed successfully! Total: $${orderPayload.TotalAmount}`);
        localStorage.setItem('orderStatus', 'paid');
        localStorage.setItem('orderTotal', orderPayload.TotalAmount);
        window.location.href = 'confirmacion.html';
    })
    .catch(error => {
        console.error('There was a problem with the fetch operation:', error);
        alert('Payment failed. Please try again.');
    });


    
    alert(`Order placed Error! Total: $${total}\nPayment will not be processed.\nRedirecting to confirmation page...`);
    localStorage.setItem('orderStatus', 'FAIL');
    localStorage.setItem('orderTotal', total);
 
    //window.location.href = 'confirmacion.html';
}
function placeOrder() {
    if (cart.length === 0) {
        alert('Your cart is empty!');
        return;
    }
    
}

