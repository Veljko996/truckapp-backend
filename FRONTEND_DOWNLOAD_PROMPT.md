# Frontend Prompt: Dugme za Download Naloga

## API Endpoint Informacije

**Endpoint:** `GET /api/nalog/{id}/document`

**Query Parametri:**
- `format` (opciono): `"html"` (default) ili `"doc"`

**Autentifikacija:** 
- Zahteva autorizaciju (Bearer token)
- Role: `Admin` ili `Korisnik`

**Response:**
- Vraća fajl za download
- Content-Type: `text/html; charset=utf-8` (za HTML) ili `application/msword` (za DOC)
- Filename: `Nalog_{id}.html` ili `Nalog_{id}.doc`

## Primeri Implementacije

### 1. React sa Axios/Fetch

```tsx
import React from 'react';
import axios from 'axios';

const DownloadNalogButton = ({ nalogId }: { nalogId: number }) => {
  const handleDownload = async (format: 'html' | 'doc' = 'html') => {
    try {
      const token = localStorage.getItem('token'); // ili kako čuvaš token
      
      const response = await axios.get(
        `/api/nalog/${nalogId}/document?format=${format}`,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          },
          responseType: 'blob' // VAŽNO: mora biti blob za download
        }
      );

      // Kreiraj link za download
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Nalog_${nalogId}.${format}`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Greška pri preuzimanju:', error);
      alert('Greška pri preuzimanju dokumenta');
    }
  };

  return (
    <div>
      <button onClick={() => handleDownload('html')}>
        Preuzmi HTML
      </button>
      <button onClick={() => handleDownload('doc')}>
        Preuzmi DOC
      </button>
    </div>
  );
};
```

### 2. Vanilla JavaScript / Fetch API

```javascript
async function downloadNalog(nalogId, format = 'html') {
  const token = localStorage.getItem('token');
  
  try {
    const response = await fetch(
      `/api/nalog/${nalogId}/document?format=${format}`,
      {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );

    if (!response.ok) {
      throw new Error('Greška pri preuzimanju');
    }

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Nalog_${nalogId}.${format}`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);
  } catch (error) {
    console.error('Greška:', error);
    alert('Greška pri preuzimanju dokumenta');
  }
}

// Primer korišćenja:
// <button onclick="downloadNalog(123, 'html')">Preuzmi HTML</button>
// <button onclick="downloadNalog(123, 'doc')">Preuzmi DOC</button>
```

### 3. React sa Fetch (bez Axios)

```tsx
const DownloadButton = ({ nalogId }: { nalogId: number }) => {
  const downloadDocument = async (format: 'html' | 'doc') => {
    const token = localStorage.getItem('token');
    
    try {
      const response = await fetch(
        `${API_BASE_URL}/api/nalog/${nalogId}/document?format=${format}`,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      if (!response.ok) {
        throw new Error('Neuspešno preuzimanje');
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Nalog_${nalogId}.${format}`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error(error);
      // Prikaži error toast/notification
    }
  };

  return (
    <>
      <button onClick={() => downloadDocument('html')}>
        📄 Preuzmi HTML
      </button>
      <button onClick={() => downloadDocument('doc')}>
        📝 Preuzmi DOC
      </button>
    </>
  );
};
```

### 4. Vue.js Primer

```vue
<template>
  <div>
    <button @click="downloadNalog('html')">Preuzmi HTML</button>
    <button @click="downloadNalog('doc')">Preuzmi DOC</button>
  </div>
</template>

<script>
export default {
  props: {
    nalogId: {
      type: Number,
      required: true
    }
  },
  methods: {
    async downloadNalog(format) {
      const token = this.$store.state.auth.token; // ili kako čuvaš token
      
      try {
        const response = await this.$http.get(
          `/api/nalog/${this.nalogId}/document?format=${format}`,
          {
            headers: {
              'Authorization': `Bearer ${token}`
            },
            responseType: 'blob'
          }
        );

        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `Nalog_${this.nalogId}.${format}`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error('Greška:', error);
        this.$toast.error('Greška pri preuzimanju');
      }
    }
  }
}
</script>
```

## Važne Napomene

1. **ResponseType mora biti 'blob'** - Bez ovoga, browser neće pravilno obraditi fajl za download
2. **Autorizacija** - Obavezno proslediti Bearer token u header-u
3. **URL kreiranje** - Koristiti `window.URL.createObjectURL()` za kreiranje privremenog URL-a
4. **Cleanup** - Uvek pozvati `window.URL.revokeObjectURL()` nakon download-a da oslobodi memoriju
5. **Error Handling** - Obavezno dodati error handling za slučaj neuspešnog zahteva

## Base URL

Ako koristiš proxy ili različit base URL, zameni `/api/nalog` sa punim URL-om:
- Development: `http://localhost:5000/api/nalog`
- Production: `https://your-api-domain.com/api/nalog`

## Dodatne Opcije

Možeš dodati loading state:

```tsx
const [isDownloading, setIsDownloading] = useState(false);

const handleDownload = async (format: 'html' | 'doc') => {
  setIsDownloading(true);
  try {
    // ... download logic
  } finally {
    setIsDownloading(false);
  }
};

return (
  <button 
    onClick={() => handleDownload('html')} 
    disabled={isDownloading}
  >
    {isDownloading ? 'Preuzima se...' : 'Preuzmi HTML'}
  </button>
);
```

