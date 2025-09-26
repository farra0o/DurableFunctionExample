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

function placeOrder() {
    if (cart.length === 0) {
        alert('Your cart is empty!');
        return;
    }
    
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
    const userName = localStorage.getItem('user_name');
    const orderItems = cart.map(item => ({
        ItemId: item.id,
        Quantity: item.quantity,
    }));

    return {
        UserEmail: userEmail,
        UserName: userName,
        Items: orderItems
    };
    
}

async function processPayment() {
    try {
        var Json = prepareOrderPayload();
        console.log("JSON to send: ", Json);

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
        console.log("Respuesta backend:", data);
        const instanceId = data.id; // 'instanceId')
        if (!instanceId) {
            throw new Error("No se recibió instanceId");
        }
        localStorage.setItem('instanceId', instanceId);
        localStorage.setItem('statusQueryGetUri', data.statusQueryGetUri); // opcional, si quieres usar ese endpoint
        })

        .catch(error => {
        console.error('There was a problem with the fetch operation:', error);
        alert('Payment failed. Please try again.');
        });

        alert(`Order placed Total: $${total}\nPayment will processed.\nRedirecting to confirmation page...`);
        localStorage.setItem('orderStatus', 'FAIL');
        localStorage.setItem('orderTotal', total);

        await OrderStatus(instanceId);
    } 
        
        catch (error) {
        console.error('Error al procesar el pago:', error);
        alert('Ocurrió un problema al procesar el pago. Por favor, intenta de nuevo.');
        localStorage.setItem('orderStatus', 'fail');
    }
}


async function OrderStatus(instanceId, interval = 2000, timeout = 60000) {
    const startTime = Date.now();
    console.log("Checking order status for instanceId: ", instanceId);
    return new Promise((resolve, reject) => {
        const poll = async () => {
            try {
                const response = await fetch(`http://localhost:7256/api/orders/status/${instanceId}`);
                if (!response.ok) {
                    return reject(`Error al consultar estado: ${response.status}`);
                }

                const data = await response.json();

                // Revisar RuntimeStatus
                const status = data.runtimeStatus;
                console.log(`Estado actual: ${status}`, data.customStatus);

                if (status === 1) {
                
                    resolve(data.output);
                } else if (status === 0 || status === 2) {
                    reject(`La orquestación terminó con estado: ${status}`);
                } else {
                    // Revisar timeout
                    if (Date.now() - startTime >= timeout) {
                        reject("Polling excedió el tiempo máximo.");
                    } else {
                        setTimeout(poll, interval); // seguir haciendo polling
                    }
                }
            } catch (err) {
                reject(err);
            }
        };

        poll();
    });
}
