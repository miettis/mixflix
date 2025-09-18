---

## 🎨 **MixFlix Color Palette**

### 🔵 **Primary Gradient Colors** (from the logo)

| Role              | Hex                                        | RGB              | Use Case                         |
| ----------------- | ------------------------------------------ | ---------------- | -------------------------------- |
| **Teal Blue**     | `#00D8C3`                                  | rgb(0, 216, 195) | Main brand color, buttons, links |
| **Violet Purple** | `#9A00D8`                                  | rgb(154, 0, 216) | Accent color, highlights         |
| **Gradient**      | `linear-gradient(90deg, #00D8C3, #9A00D8)` | —                | Logo, call-to-action areas       |

---

### ⚫ **Background / Dark Mode**

| Role              | Hex       | RGB             | Use Case                    |
| ----------------- | --------- | --------------- | --------------------------- |
| **Dark Charcoal** | `#0A0A0A` | rgb(10, 10, 10) | Background, cards, overlays |
| **Soft Black**    | `#121212` | rgb(18, 18, 18) | App background, nav bars    |

---

### ⚪ **Text & UI Neutrals**

| Role           | Hex       | RGB                | Use Case                   |
| -------------- | --------- | ------------------ | -------------------------- |
| **White**      | `#FFFFFF` | rgb(255, 255, 255) | Primary text               |
| **Light Gray** | `#CCCCCC` | rgb(204, 204, 204) | Secondary text             |
| **Dim Gray**   | `#888888` | rgb(136, 136, 136) | Tertiary, placeholder text |

---

### ✅ **Support Colors (optional)**

| Role              | Hex       | Use Case         |
| ----------------- | --------- | ---------------- |
| **Success Green** | `#00C67A` | Positive state   |
| **Error Red**     | `#FF3366` | Errors, warnings |

---

## 🧪 Bonus: Tailwind CSS Config Snippet

If you're using **Tailwind**, here’s how you could define custom colors:

```js
// tailwind.config.js
theme: {
  extend: {
    colors: {
      mixflix: {
        teal: '#00D8C3',
        purple: '#9A00D8',
        bg: '#0A0A0A',
        text: '#FFFFFF',
        secondary: '#CCCCCC',
        accent: '#FF3366',
      },
    },
  },
},
```

---
