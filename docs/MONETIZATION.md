# Gelir ve kalıcı ekonomi — araştırma / tasarım taslağı

2026-10-08. Kullanıcının fikirleri ve araştırmadan çıkarılan öneriler ayrı tutulur. Henüz reklam/IAP SDK'sı, kalıcı hesap cüzdanı, ürün veya sonuç ödülü uygulanmadı. M7a `Wallet` yalnız mevcut koşunun parasıdır; uygulama kapanınca kayıt M8 işidir.

## Kullanıcının hedeflediği akış

- Battle: ölüm/koşu sonucu ödülünü isteğe bağlı video ile 3× alma.
- Hikâye: ölümde isteğe bağlı video ile devam; devam seçilmezse sonuç ödülünü video ile 3× alma.
- Ödüllü reklam reddedilirse geçiş reklamı fikri; ayrıca ana menüde uygun fiyatlı zorunlu reklam kaldırma ürünü.
- Hikâyede istasyonlar arasında ara sıra dron gibi bir yardım teklifi. Drona tıklamak ücretsiz teslim mi, reklam açmak mı demek henüz seçilmedi; bir anda reklam açılacağı varsayılmaz.
- Az sayıda anlaşılır kaynak/ürün; Bullet Echo hissi referans, karmaşık teklif/para birimi yığını hedef değil.

## Doğrulanmış örnekler ve sınırları

Kaynaklar 2026-10-08'de kontrol edildi. Bir ürünün kullanımda olması bizim oyunda aynı geliri sağlayacağının kanıtı değildir. Kamuya açık yardım sayfaları Bullet Echo'nun güncel kârını, oyuncu edinme maliyetini veya özel A/B sonuçlarını vermez.

- [Bullet Echo — Main resources](https://zepto.helpshift.com/hc/en/10-bullet-echo/faq/1027-main-resources/?s=account): coin ve premium Bucks yanında çok sayıda geliştirme/etkinlik kaynağı var. Alınacak ders: kaynakların ayrı amacı olmalı. Bu kadar çok türü kopyalamak mevcut sade hedefe uymaz.
- [Bullet Echo — Star Pass](https://zepto.helpshift.com/hc/en/10-bullet-echo/faq/1423-star-pass/): oyun oynayarak ilerleyen ücretsiz/ücretli ödül yolları ve bölüm bazlı satın alma sunuyor. Bizde sezonluk içerik kapasitesi olmadan pass açmak sürekli içerik borcu yaratır.
- [Bullet Echo — Skins](https://zepto.helpshift.com/hc/en/10-bullet-echo/faq/1111-skins-how-to-get-them/): kozmetik görünüm satışları ve farklı kazanma yolları var. Bizim karakter/silah/dron görünümleri için doğrudan anlaşılır ürün örneği; satış başarısı garantisi değildir.
- [The Tower — 26 Ağustos 2026 v29 notları](https://techtreegames.zendesk.com/hc/en-us/articles/54972978743707-v29-Patch-Notes-2026-08-26): reklam gem ödülleri ve Disable Ads ile talep etme hâlâ kullanılıyor; günlük talep limiti de bulunuyor. Bu oyunun reklam kaldırma kapsamı bizim yalnız geçiş reklamını kaldırma fikrimizden farklı. Ürün açıklaması kapsamı açık söylemeli.
- [Google / Refuel Games vaka çalışması](https://admob.google.com/home/resources/refuel-games-grows-ad-revenue-with-admob-rewarded-ads/): Rally Fury'de isteğe bağlı para/booster ödülleri, beta ve oyuncu geri bildirimiyle değerlendirildi; reklam gelirinde %17 artış raporlanıyor. Eski ve reklam sağlayıcısının yayımladığı tek oyun örneği; 2026 sektör ortalaması veya bizim gelir tahmini değildir.

## Önerilen sade başlangıç — henüz seçilmiş tasarım değil

| Katman | Öneri | Kullanım |
| --- | --- | --- |
| Koşu kaynağı | Hurda / mevcut oyun parası; sadece koşu boyunca | Silah, mermi, kapı, turret, dron. Yeni koşuda sıfırlanır. Battle sırasında gerçek parayla anlık güç satılmaz. |
| Kalıcı kazanım | Tek bir kazanılabilir bilet/kredi; adı ve harcamaları M8'de seçilir | Sonuç/başarı ödülü, görünüm açma; hikâye kalıcı geliştirmesi seçilirse bunun kapsamı ayrıca belirlenir. Battle gücüyle otomatik birleşmez. |
| Gerçek para ürünleri | Zorunlu reklam kaldırma + küçük kozmetik/başlangıç görünüm paketi | İlk sürüm için doğrudan fiyatlı ürünler yeterli olabilir; premium para zorunlu değil. |

İkinci kalıcı premium para ancak birden fazla anlamlı harcaması oluştuğunda eklenmeli. İki kalıcı cüzdan + koşu parası aslında üç kaynak demektir; arayüzde aynı anda gösterip karmaşa yaratmayalım. Şu anda premium taş, sandık anahtarı, parça, enerji ve aboneliği birlikte ekleme önerilmiyor.

Ürün sırası önerisi: zorunlu reklam kaldırma → karakter/silah/dron kozmetik paketi → içerik üretim kapasitesi oluşursa küçük, tek ücretsiz/ücretli yollu sezon bileti. Karakterler eklenirse küçük yan yeteneklerle farklı oyun tarzı verebilir; ücretli karakterin daha güçlü olması Battle adaletini etkiler. Önce ortak denge ve ücretsiz kazanma yolu gerekir. Sandıklar ne vereceği belli koleksiyon ekonomisi olmadan anlamlı değil; ilk sürümde açık içerikli doğrudan ürün daha anlaşılır.

## Reklam yerleşimi önerileri

- Battle 3× yalnız kalıcı sonuç ödülüne uygulanır; yarışın sırasını veya aktif koşu parası/hasarını değiştirmez. Örneğin temel sonuç 100 ise tamamlanan reklam toplamı 300 yapar; zaten verilen 100 üzerine 300 daha eklenmez. XP, görev ve başka ödüllerin otomatik çarpılması ayrıca seçilmelidir.
- Hikâyede ilk deneme: koşu başına en fazla bir video devamı; devam edilirse sonuç henüz teslim edilmez. Nihai bitişte en fazla bir 3× teklif. Aynı ölüm ekranında iki reklamı arka arkaya zorlamayalım. Battle'da ölümden dirilme eleme kuralını bozacağından önerilmez.
- Dron yardım teklifi yalnız güvenli geçiş anında, açık ödül ve etkileşimle; sıklık/koşu sınırı ayarlanabilir. Varsayılan olarak savaşta popup açılmaz. Normal mağaza dronunun limit/mermi/teslim API'sini paylaşır.
- Ödüllü reklamı reddetmek otomatik “oyuncu reklam izlemez” sınıflandırması yapmaya yetmez. Reddin hemen ardından zorunlu reklam açma önerisini almayalım. Geçiş reklamı bağımsız sıklık/oturum sınırıyla, sonuçtan menüye doğal geçişte değerlendirilsin; az önce ödüllü video izlendiyse ek reklam gösterilmesin.
- Reklam kaldırma ürünü yalnız zorunlu geçiş reklamını kaldıracaksa adı/açıklaması bunu söylemeli; isteğe bağlı ödüllü videoların kaldığı açık olmalı. Reklam izlemeden bütün bonusları teslim eden daha geniş ürün başka bir fiyat/değer önerisidir.
- Reklam bulunamazsa/hata/erken kapatma olursa temel ödül ve normal devam düğmesi çalışır; sonradan savaşın ortasında reklam açılmaz.

[Google ödüllü reklam kuralları](https://support.google.com/adsense/answer/9121589?hl=en) açık seçim/ödül bildirimi ve vaat edilen ödülün teslimini ister; reddetmek normal uygulama kullanımını engellememelidir. [AdMob geçiş reklamı kılavuzu](https://support.google.com/admob/answer/6201362?hl=en) beklenmedik ve sık tekrarlanan gösterimlerden kaçınmayı, doğal içerik aralarını kullanmayı belirtir. Bu kaynaklardan çıkardığımız tasarım önerisi, reddetmeye bağlı ceza reklamı yerine bağımsız ve sınırlı doğal geçiş yerleşimidir. Uygulama/yayın tarihinde politikalar tekrar kontrol edilir.

## Geliştirme sırası ve veri sözleşmesi

M7b turret → M7c dron → Mod/Bot ve Battle elemesi → M8 koşu sonucu, meta harcamaları ve kayıt → M9 reklam/IAP adaptörleri. Gelir tasarımı M8 öncesi netleşir; servisi erken eklemek çekirdek kusurlarını çözmez.

`RunId`/sonuç kimliği, kesinleşmiş temel ödül, tek teslim/çarpan durumu; kalıcı cüzdan ve zorunlu reklam kaldırma hakkı kayıt şemasında ayrı tutulmalı. Koşu cüzdanı sonuç hesabına doğrudan dönüştürülmemeli: kazanılan ve harcanan para farklıdır. IAP işlemi ve reklam reward callback'i tekrar gelebilir; teslim aynı işlem için bir kez yapılmalı. SDK reklamı kapandı sinyali tek başına ödül kazanımı değildir. Uygulama kapanması/yeniden açılması ve satın alımları geri yükleme kendi aşamasında doğrulanır.

İlk ölçümler: oyuncunun sonraki koşuya başlaması/geri gelmesi, reklam teklif kabulü/tamamlanması, reklam sonrası çıkış, satın alma dönüşümü ve kullanıcı başı gelir. Gelir kadar oyuncu kaybı da izlenir. Kesin ürün fiyatı, reklam aralığı, 3× kapsamı ve kazanç hedefi bu taslaktan çıkarılmış kullanıcı kararı değildir.
