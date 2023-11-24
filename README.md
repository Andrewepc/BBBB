# Browser Based Battle Bowl

##Requerimientos
<ul>
  <li>Editor de Unity 2022.3.14f1</li>
  <li>NPM</li>
</ul>
  
##Installacion
<ol>
  <li>Descargar el repositorio.</li>
  <li>Abrir el proyecto del cliente en Unity y establecer la direccion del servidor destino en el controllador de WebSocket o atarla a un variable de entorno.</li>
  <li>Hacer build al proyecto, asegurandose de deshabilitar compression gzip.</li>
  <li>En el folder del sorvidor, abrir una terminal e instale los paquetes de Node con npm install.</li>
  <li>Copiar el build de Unity dentro del folder del servidor.</li>
  <li>Inicie el servidor con npm start y acceda a la pagina principal con su navegador.</li>
</ol>
