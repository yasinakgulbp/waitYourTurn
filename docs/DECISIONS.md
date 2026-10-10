# Karar kaydı

## 2026-10-10 — D52: maket sineması ve PC kasması

Kullanıcı minyatür/tilt-shift hissi, karanlık uzak çevre, mobil maliyet sınırı ve tren parça/titreşim incelemesi istedi. Survival'a sınırlı düşük çözünürlüklü blur, renk/vignette, analitik çevre karartma ve65ms kamera yumuşatma kuruldu. Gövdeler/körükler zaten birleşikti;802 eski kapalı görsel componenti fizik/nav korunarak silindi. PC kasması kilitli altı-prefab Inspector ile ilişkili bulundu; kilit açıldıktan sonra24AI yürüyüşte efekt açık ortalama5.31–6.07ms. Bu cihaz kabulü değildir.15kapı/15cam/iki geçiş kısa kabulü PASS. Battle sahnesi değiştirilmedi. Ayrıntı ve ölçüm sınırları [MAQUETTE_CINEMA.md](MAQUETTE_CINEMA.md); sonraki iş kullanıcı görünüm değerlendirmesi ve maket karakter/zombi giydirme, ardından A54 kalabalık kontrolü.


## 2026-10-10 — D51: mimari maket sanat denemesi

Kullanıcı D50'nin dokusuz treninden devam ederek beyaz kaliteli maket kartonu,
grafit raylar, özgün teknik zemin çizimleri, kesilmiş kâğıt ağaçlar ve ölçülü
bordo vurgular istedi. Yeni referanslar yalnız sanat dili; tren şekli/ölçüleri,
kapı yerleri ve mekanikler değişmez. Mevcut Survival gövdesinin görünümü,
sahneye özel prop malzemeleri ve mevcut hareket otoritesinin görsel çevresi
uyarlanır. Battle sahnesi ayrı kalır. Tek hafif shader + offline köşe AO;
realtime gölge/probe/DOF/SSAO eklenmez. Önce Unity kamerasında görsel deneme,
kısa mevcut mekanik kontrolü ve kullanıcı değerlendirmesi; A54 sanat testi
ayrı kabul kapısıdır. Reklam CTR/gelir veya FPS artışı kabul edilmiş sayılmaz.
Kaynak/ölçü/yeniden kurulum ve kanıtlar [MAQUETTE_ART.md](MAQUETTE_ART.md).

## 2026-10-10 — D50: önceki tren, yüzey dokusuz

Kullanıcı yeni doku fikrini daha sonra iletmek üzere **önceki tren modelini oyunda dokusuz kullanmayı** istedi. D49 look-development yönü kabul edilmiş değildir; çalışma kaynak olarak saklanır, sahneye kurulmaz. Survival'daki mevcut D48 geometri/kapı/cam/körük modeli korunur. Gövde düz gri, yapraklar düz kırmızı, körük koyu düz Standard materyal kullanır; yeni opak materyallerin bütün yüzey texture slotları boştur. Cam ve mevcut portal görsel bağları kalır. Collider/nav/meshler ve Battle sahnesi değişmez; yeni fikir gelene kadar yüzey dokusu geliştirme durur. Editor komutu `Use Previous Train Without Textures` yeniden uygulanabilir. Mevcut textured kurulum komutu eski atlası tekrar bağlar; kullanıcı yeni fikrini vermeden onu kullanmayın.

## 2026-10-10 — D49: önce malzeme ve ışık denemesi

Kullanıcı D48 görünümünü önceki tasarımdan da kötü buldu; nötr çelik / desensiz zemin yönü reddedildi. Hedef hâlâ 09.44.05 referansındaki sıcak eskitilmiş iç metal, koyu ince desenli güverte, kırmızı kapılar, soğuk gümüş çerçeve ve sıcak ışık vurgularıdır. Kullanıcı dokuları kendimiz üretip model üzerinde bitirmeden denememizi istedi. Önceki atlaslar da dışarıdan indirilmiş değildi; üretildi, fakat oyundaki sonuç yeterli değildi.

`ArtSource/SurvivalTrain/LookDev` Assets dışında ayrı bir Blender malzeme/ışık çalışmasıdır. Mevcut metre ölçülü gövde üzerinde fiziksel ölçekte özgün zemin deseni, küçük aşınmalar ve render ışığı değerlendirilir. İlk iki native node denemesinden sonra doğal metal aşınması için built-in ImageGen ile özgün dört hücreli yüzey atlası üretildi; kaynak ve tam prompt aynı klasörde korunur. Atlas metre bazında yüzeye oturur; zemin/kabartma/roughness node ile üretilir. Gerçek 3D render, dört ham renk örneği ve düzenlenebilir .blend teslim edilir; oyun sahnesi/atlas/kurucu bu adımda değiştirilmez. Koltuk/bölme siluetleri yalnız görsel öneridir, oyun colliderı değildir. Cycles shader/ışıkları mobil runtime'a doğrudan taşınmayacak; yön kabul edilince kompakt atlas/bake ve önce tek-vagon Unity görünüm kontrolü yapılacaktır. Blender görüntüsü Unity veya Android kalite/performance kabulü değildir. Kullanıcı görünüm değerlendirmesi açık; D48 teknik mekanik kayıtları sanat kabulü sayılmaz.

## 2026-10-10 — D48: oyun içi tren görünümü ve yüzey kararlılığı

Kullanıcı D47'nin oyun içi dokusunu/zeminini ve kamera hareketinde özellikle körükte görünen titremeyi kabul etmedi. Görünüm için sarımsı iç panel yerine nötr çelik, sakin koyu güverte ve daha okunur metal ışığı hedeflenir. Ana gövde zaten tek mesh; körük-vagon arasında aynı düzlemde örtüşen görsel yüzeyler kaldırılır. Fizik/nav güvenli bindirmesi korunur. Mevcut tek gölgesiz yönlü ışık ayarlanır, ek ışık/gölge/DOF yoktur. Kaynak .blend/atlas ve yeniden kurulum kodu birlikte güncellenir. Dört birleşimde coplanar art örtüşme kontrolü mesh preflight'e eklendi. Yeni tren24.660 üçgen;39 renderer. Kullanıcı görünüm kabulü ve Android ölçümü ayrıca gereklidir; referans atmosferi tamamlanmış sayılmaz.

## 2026-10-09 — D47: referansa dayalı tren sanatı

**Devam / güncel durum:** Kullanıcı Blender önizlemesinden sonra ilk tasarımı Survival'a giydirmemizi istedi. Kurulum yapıldı; 15 kapı/cam/metal ve iki körük için kısa PC kabulü PASS. Hareket/nav kaynağı aynı; Battle sahnesi değişmedi. Sarımsı pencere panelleri ve zemin sonraki görsel düzeltmeler için not edildi. Android FPS/termal/pil ölçümü henüz yok. Aşağıdaki önizleme bekleme notu önceki teslim anının kaydıdır.

Kullanıcı ilk sarı çizgili Blender taslağını reddetti: oyun blokout'u nihai görsel tasarım değildir; yalnız ölçüler korunur. Renk/doku/desen, iç-dış metal ve pencere oranları kullanıcının kırmızı kapılı metro referansına yaklaştırılacak. Üç Survival vagonu ve geniş açık körükler korunur. Kullanıcı bu turda **önce Blender tasarımını görmeyi** seçti; Unity sahnesine henüz kurulum yapılmaz. Yeni gövde, paketli kaynak ve gerçek Blender renderi hazır; görünüm değerlendirmesi bekler.

Yeni pencere yüksekliği ve kapı yaprak çerçeveleri yalnız Survival sanat varyantına aittir. Metal görünümüyle atış colliderı eşlenecek; hareket/nav/kimlikler ve Battle sahnesi korunacak. Teknik aktarım kodu hazır/derlendi, offline mesh önkontrolü geçti; Unity shader/Play/cihaz kabulü henüz yapılmadı. Ayrıntı ve ölçüler [TRAIN_ART.md](TRAIN_ART.md). Bu kayıt sanat kalitesi veya FPS garantisi değildir.

Tarih: 2026-10-07. `Kararlaştırıldı` kullanıcı tarafından netleştirilen tasarım veya görev kapsamıdır. `Teknik öneri` uygulanmadan önce ilgili aşamanın testinde doğrulanır; kullanıcı onayıymış gibi sunulmaz.

## Kararlaştırılanlar

| Kimlik | Karar | Gerekçe / etkisi |
| --- | --- | --- |
| D01 | Modüler, az bağımlı mimari; küçük ve doğrulanmış aşamalar | Ana mekanikler birbirine uyumlu gelişecek; tek seferde tüm oyun yazılmayacak. |
| D02 | Gameplay tren zemini sabit; çevre görsel olarak hareket eder | Kullanıcı seçti. Hareketli platformun navigasyon/physics yükü ilk kapsamda yok. |
| D03 | İlk istasyon savunması 60 saniye; süre veriden değişir | Kullanıcı seçti. Tüm zombilerin ölmesi beklenmez. |
| D04 | İçeri giren zombiler trende kalır; dışarıdakiler istasyonda kalır | Kullanıcı seçti. Yolculuğun görünen bölümünde tehlike devam eder; içeride/dışarıda üyelik takip edilir. |
| D05 | Can, para, silah ve dron oyuncuyla gider; kapı hasarı/turretler vagonlarında kalır | Kullanıcı seçti. Vagonların durumları ayrı ve koşu boyunca kalıcıdır. |
| D06 | Karanlık geçişte oynanış durur; görünür yolculukta savaş sürer | Kullanıcı seçti. Kontrol/görüş olmayan sürede haksız hasar ve kaynak üretimi yok. |
| D07 | İyileştirme mevcut canın %50'si, minimum 30; maksimum canla sınırlı | Kullanıcı seçti. 10/100 → 40/100, 50/100 → 80/100. |
| D08 | Kapılar hasarla kırılır; vagonlara göre maksimum canları farklıdır | Kullanıcının ana mekaniği. Prototipteki zamanlı kapılar yeni davranışla değiştirilir. |
| D09 | Kırık kapıya yakın kalınca yaklaşık 3 saniyede onarım; ayrılınca iptal | Kullanıcının ana mekaniği. Süre ayarlanabilir; yarım onarım kapıyı tamamlamaz. |
| D10 | Turret ve dron sınırlı mermi kullanır; dron oyuncuyu izler ve mermi bitince kamikaze yapar | Kullanıcının ana mekaniği. Satın alma sınırları ve hedef kaybı yönetilir. |
| D11 | İngilizce, aşamalara ait küçük commitler; repo içi ilerleme kaydı | Kullanıcının çalışma tercihi. Kaynak koddaki mevcut değişiklikler plan commitine alınmaz. |
| D12 | Dış incelemeler öneridir; değişiklikler oyun hedefi ve somut gerekçeyle seçilir | Kullanıcının tercihi. Model uzlaşması veya nezaket tasarım kararının kanıtı değildir. |
| D13 | Sol joystick ile hareket, en yakın görüşü açık zombiye otomatik nişan ve ateş | Kullanıcı M3 için seçti; masaüstünde WASD/ok tuşları aynı hareket girdisine bağlanır. Sağ joystick gerekmez. |
| D14 | İlk tabanca sınırlı şarjör, sınırsız yedek mermi ve otomatik doldurma kullanır | Kullanıcı M3 için seçti. İlk lab değerleri deneme ayarıdır: 8 mermi, 1.2 s doldurma, 0.35 s atış aralığı. |
| D15 | Sağlam kapının üst camından dışarıdaki zombilere ateş edilir; alt dolu panel ve vagon metalleri atışı keser | Kullanıcı M3 değerlendirmesinde netleştirdi. Oyuncu ve ileride kapı önündeki turretler zombileri kapı kırılmadan vurabilir. Kapının yürüyüş engeli ile atış engeli ayrıdır. Başlangıçta tüm yan duvar opaktı; D24 yan pencere açıklıklarını ayrıca geçirgen yapar. |
| D16 | Oyuncu ilk kapsamda atandığı vagondan dışarı yürüyemez; kapı kırılması bu izni değiştirmez | Kullanıcı M3 değerlendirmesinde seçti. Hareket alanı ayrı ve isteğe bağlıdır; ileride istasyona inme görevi bu sınırı kaldırabilir/değiştirebilir. Zombi girişini veya atışı engelleyen ek fizik duvarı kurulmaz. |
| D17 | Kayıt hedefi mevcut koşuya devam, ölümde yeni koşu; kalıcı geliştirmeler ayrıca tasarlanır | Kullanıcının ilk seçimi; ardından sağlayıcı/ayrıntılarda emin olmadığını belirtti. Google Play Games Saved Games adaydır, kesin sağlayıcı seçilmedi ve M4'te disk/bulut entegrasyonu yapılmaz. |
| D18 | Ortak temel üzerinde botlu hayatta kalma yarışı (Battle) ve botsuz tek oyunculu ilerleme/hikâye modu hedeflenir | Kullanıcının yeni vizyonu: tren giriş sunumu, nickli komşu vagon rakipleri, son hayatta kalan/sıralama; en az 3 bot beceri profili. Gerçek ağ oyuncuları bu talepten çıkarılmaz. M4a önce ortak döngüyü kanıtlar; mod/bot aşaması ayrı kurulur. |
| D19 | Tabanca sınırsız yedek; hafif makineli, tüfek ve pompalı toplam sınırlı mermi kullanır | Kullanıcı M5'te seçti (2026-10-08). Şarjör bitince yedek varsa otomatik doldurma; son dolum kısmi olabilir. Yeni silah alma/mermi ürünü M7'de tasarlanır; silah veya vagon değiştirmek mermi vermez. |
| D20 | Varsayılan: oyuncu hasarı onarımı kesmez, kapı hasarı ilerlemeyi sıfırlar; iki kural ayrı ayarlanabilir | Kullanıcı 2026-10-08'de bu ayrımı netleştirdi. Hasar engellenmez; kapı kırılabilir ve oyuncu ölebilir. Onarım süresi/menzili ve iki kesilme anahtarı denge araçlarıdır. Dalga bitişi koşulu yoktur. |
| D21 | Zombiler trenin iki tarafından da gelebilir | Kullanıcı 2026-10-08'de hatırlattı. Tek taraflı/tek kapılı lab üretim hedefi değildir. Gerçek vagon entegrasyonunda her iki tarafın yürünebilir alanı, kapıları ve spawn verileri doğrulanır; sunum hareketi saldırı tarafına bağlı olmaz. |
| D22 | Nihai görünüm/yerleşim referansı ilk prototiptir; lab sahnesi onun yerine geçmez | Kullanıcı 2026-10-08'de açıkça yeniden belirtti: uzun dikdörtgen vagon, oyuncuyu izleyen prototip kamera yaklaşımı, iki tarafta istasyon alanı ve çok kapı. M6a gerçek geometri/kamera entegrasyonu yeni özelliklerden önce doğrulanır. Somut lab varsayımları VISION_AND_INTEGRATION.md içinde. D23 yeni kamera/ölçek görselleriyle oranları günceller; eski prefab ölçüleri zorunluluk değildir. |
| D23 | Yeni üç görsel kamera/ölçek/atmosfer referansı; vagon başına 4–6 kapı ve aynı gövde ölçüsü | Kullanıcı 2026-10-08: eski prototipteki oransız boyutlar korunmasın, yönü belli kapsüller kullanılabilir; görsellerde yalnız tarif edilen özellikler referanstır. Kapı sayısı ve dayanıklılık ayrı şans/zorluk araçlarıdır; ortadaki bir/iki kapı duvarla değişebilir. Telefon/tablet kadrajı uyarlanır. Deneme ölçüleri, model bağlama ve kanıt VISUAL_SCALE.md içindedir. |
| D25 | Mağaza savaşta ve görünür yolculukta kullanılabilir; açıkken oyun sürer | Kullanıcı M7a'da seçti. Karanlık geçişte ve ölümde satın alma kapanır; vagon değişimi eski panel bağlamını geçersiz kılar. |
| D26 | Silah ilk alımda açılır ve dolar; tekrar satın alma eksik mermiyi aynı fiyatla tamamlar | Kullanıcı M7a'da seçti. Doluyken para harcanmaz; satın alma atış beklemesini sıfırlamaz. Tabanca ücretsiz başlangıç silahıdır. |
| D27 | Yaşayan insan/bot savunmacı olmayan vagona yeni zombi üretilmez; içeride kalanlar korunur | Kullanıcı M7a değerlendirmesinde boş/elenmiş vagonların her istasyonda dolup kaçınılmaz ölüm yaratmasını istemedi. Uygunluk görünürlüğe veya sadece insanın seçili vagonuna bağlanmaz. |
| D28 | Atamada zombilerden en uzak fiziksel boş aday + ayarlanabilir 2 sn hasar koruması | Kullanıcı soruda onayladı. Karanlıkta süre tüketilmez; ateş/onarım sürer. Sayısal denge ileride değişebilir. |

## Teknik öneriler

| Kimlik | Öneri | Doğrulama aşaması |
| --- | --- | --- |
| T01 | Yakın istasyon/vagon zeminlerinde kesintisiz NavMesh + kapıda carving | İlk link deneyi A54'te doğrulandı; kullanıcı düzenli ikili giriş ve uzak köprü geometrisini uygun bulmadı. 2026-10-07 revizyonu: kapı vagon kenarında, zeminler 0.05 m aralıklı ve kısa eşikle bağlı; sabit geçiş hatları kaldırılır. Gerçek ayrık geometride link seçeneği ayrıca değerlendirilebilir. |
| T02 | Kapı/zombi/oyuncu için ortak hasar ve sağlık sözleşmesi | M2: sağlık sınırları, tek ölüm/ödül, farklı kaynaklar. |
| T03 | İlk silahlar hitscan/pellet; satın alınmış projectile assetleri VFX | M2/M3: tabanca temeli; M5: çeşitlilik. Yavaş projectile gelecekte ayrı resolver kullanır. |
| T04 | Küçük enemy durum makinesi, veri tabanlı tür farklılıkları | M3/M6: türler aynı döngüyü kullanır; gereksiz framework eklenmez. |
| T05 | Spawn canlı sınırları, havuzlar ve dağıtılmış AI/path güncellemeleri | M6/M9: cihazda ölçüm; sayısal bütçeler profil sonucuyla belirlenir. |
| T06 | D31 ile oyuncunun konumuna turret kurulumu; vagon başına havuz/kota, otomatik para ödülü | Sabit pad önerisi kullanıcı geri bildirimiyle kaldırıldı. Fiziksel para toplama ileride eklenebilir. |
| T07 | Rastgele atamada eşit vagon olasılığı; aynı vagon tekrar çıkabilir | M4: tasarım denemesi. Kullanıcı henüz bu dağılımı seçmedi. |
| T08 | Ölümde oyun biter; kararma/onarım/satın alma eski koşuyu yeniden başlatamaz | M2/M4: ölüm her evrede geçerli. |
| T09 | M1'de kaba Android kalabalık ölçümü, maliyetlerin ayrılması | Ölçüm olmadan NavMesh değiştirilmez veya mobil performans kanıtlandı denmez. |
| T10 | Minimum mobil kontrol/tabanca M3'e alınır; kullanıcıyla oynanış değerlendirmesi | M3: hareket–ateş–onarım deneyimi; M5: kontrol iyileştirme/silah çeşitliliği. |
| T11 | M4a iki vagonla döngü; M4b beş vagona genişletme | Önce atama/kalıcılık/yolculuk kanıtlanır, sonra geometri ve yük genişler. |
| T12 | Botlu yarış nick/karar/hareketle canlı hissi verir; bilgisayar rakipleri olduğu anlaşılır | Gerçek çevrimiçi insan eşleşmesi diye sunmak güven beklentisini bozabilir. Bu açıklık teknik öneridir; kullanıcı tarafından ayrıca seçilmiş sayılmaz. |
| T13 | Hasarlı sağlam kapı da 3 saniyelik yakınlık onarımıyla tam cana döner | 2026-10-08: kullanıcı bu eksik durumların ele alınmasını istedi. Hasar kaynaklarının kesilme kuralları D20 ile ayrı netleştirildi. Yarım süre can vermez; sağlam kapıda yaşam kimliği korunur, kırık kapı açıkça yeniden kurulur. |
| T14 | D02 görsel hareketi istasyon zemini/çevresini de kapsar; istasyon traverslerle döngülenmez | 2026-10-08 kullanıcı geri bildirimi: yalnız ray hareketi yetersizdi. Sabit istasyon colliderının görünümü ayrı köke taşınır; duruşta tam hizalanır. Yol çevresi sınırlı döngü, istasyon tek yaklaşma/kalkış hareketi kullanır; yeni istasyon tam karanlıkta yerleştirilir. Spawn/nav bu görsellere bağlı değildir. |

## Uygulama öncesi veya ilgili aşamada netleştirilecekler

Bu liste ilk navigasyon/hasar denemesini engellemez. İlgili özellik başlamadan karar verilir; yanıt alınmamış tercih kesinleşmiş gibi uygulanmaz.

| Konu | İlk öneri / açık detay | Gerektiği aşama |
| --- | --- | --- |
| Hedef Android cihazları | En az bir düşük ve bir orta sınıf gerçek cihaz seçmek; ölçüm olmadan sabit FPS garantisi yok. | M1 ölçüm düzeni, M6 ilk kalabalık bütçesi |
| Vagonlar arası geçiş | D16: oyuncu atandığı vagonda kalır, kırık kapıdan istasyona çıkamaz. Gelecekte görev bazlı inme izni açıkça verilir. Zombilerin başka vagona gidip gitmeyeceği ayrı. | M3/M4 |
| Onarımda kapanış ve saldırı | Geçitte karakter varsa kapanış bekler; ateş edilebilir. D20: varsayılan oyuncu hasarında kesilmez, kapı hasarında sıfırlanır; iki kural ayrı anahtarlardır. Sayısal denge oynanış değerlendirmesine açık. | M3 devamı / denge |
| Doğuş güvenliği | D28: geçerli adaylardan en yüksek düşman mesafesi + ayarlanabilir 2 sn hasar koruması kullanıcı tarafından seçildi; içerideki zombiler korunur. Gerçek oynanış dengesi açık. | M7b öncesi / denge |
| Kalkış uyarısı | İlk öneri 60 saniye savunma sonrasında 3 saniye uyarı; fade/yavaşlama süreleri ayrı ayarlar. | M4 |
| Düşman hedef önceliği | Atanmış vagonda oyuncu, turret veya başka hedeflerin önceliği; boş vagondaki zombi davranışı. | M3/M7 |
| Savunmaların canı ve patlaması | D29: turret hedef alınmaz; mermi bitişi yakındaki zombilere hasar verir. Dron davranışı M7c öncesi ayrıca seçilir. | M7 |
| Turret sınırının kapsamı | D29: vagon başına her türden en fazla 2; global koşu kotası eklenmedi. | M7 |
| Mobil kontrol hissi | Şema D13 ile seçildi; sol joystick ve otomatik ateşin ergonomisi kullanıcı/cihaz denemesinde değerlendirilir. | M3 değerlendirmesi |
| Silah çeşitlerinin mermisi | D19 sınırlı yedek; D26 ilk alım açar, tekrar alım eksik mermiyi aynı fiyatla tamamlar. Sayısal denge açık. | M7/M8 |
| Mağaza erişimi | D25 ile savaşta açık, oyun sürer; karanlık/ölümde kapalı. Mobil ergonomi ayrıca değerlendirilecek. | M7 değerlendirmesi |
| Para toplama | Kullanıcı para toplama istedi; ilk öneri ödülü otomatik cüzdana yazmak. Görsel pickup/yerden toplama tercihi açık. | M7 |
| Koşu devamı / metagelişim | Hangi kazanımların kaldığı ve devam beklentisi erken kâğıt üzerinde seçilir; disk kaydı ihtiyaç kadar uygulanır. | M4a öncesi tasarım, M8 uygulama |
| Ödüllü reklam / IAP'nin ekonomi rolü | Kullanıcının Battle sonuç 3×, hikâye devam/3×, reklam kaldırma ve yardım teklifi fikirleri alındı. Kaynaklı öneriler MONETIZATION.md içinde; kalıcı ödül, ürün, fiyat ve tekrar sınırı henüz seçilmedi. | M7/M8 tasarım, M9 entegrasyon |

## 2026-10-08 — D24: yan pencerelerden atış

İki taraftaki yan pencerelerden ateş edilir; metal çerçeve/dikme/alt-üst paneller mermiyi durdurur. Kullanıcının D23 sonrasındaki açık isteğiyle önceki dekoratif/opak pencere davranışı değiştirilir. Pencereler zombi veya oyuncu geçişi değildir; cam kırılması/onarımı eklenmez. Oyuncu ve ileride savunmalar ortak raycast geometrisini kullanır. Modelcinin pivot, ölçek, net cam ve metal hacimleri [MODEL_CONTRACT.md](MODEL_CONTRACT.md) ve üretilen ölçü CSV'sinde kayıtlıdır. Doğrulama: dört silahla tüm pencereler, metal kenarlar/dikmeler, eğik raylar ve pencere üzerinden beden/nav geçişinin kapalı olması; mevcut istasyon/kalıcılık kabulü de tekrar çalıştırılır.

## Bilinen tasarım riskleri

- M7a'da boş vagonlara da yeni üretim yapılması kullanıcı oyununda ani ölüm yarattı; D27 uygunluğu ve D28 doğuş koruması M7b öncesi ele alınır. Botlar bu temel kurala bağımlıdır, sorunun geçici çözümü sayılmaz.
- Battle sonucu ödül 3× ve hikâyede reklamla devam/3× kullanıcının hedef fikirleridir; tekrar sınırı/kalıcı ödül/fiyat netleşmedi. D30 kullanıcı tercihi reddin ardından kısa sonuç geçiş reklamıdır; bağımsız yerleşim eski teknik öneridir, seçilmiş karar değildir. Araştırma/alternatifler MONETIZATION.md içinde.

- Kalıcı kapı hasarı ve rastgele vagon ataması, oyuncuyu çok tehlikeli bir vagona getirebilir. Bu istenen risk/şans hissini destekler; anında kaçınılmaz ölüm olmaması için doğuş güvenliği gerekir.
- Turretler eski vagonlarda kaldığından oyuncu dışında öldürme/ödül devam edebilir. Karanlıkta tüm oynanış durur; görünür zamanda bu etki ekonomi dengesiyle değerlendirilir.
- Süreli kalkışta içeride kalan zombi sayısı her istasyonda artabilir. Global canlı sınırı sadece yeni spawnı değil taşınan zombileri de saymalıdır; içeri girmiş zombi sırf bütçe doldu diye yok edilmez.
- Kapıyı onarıp tekrar kırdırmanın ücretsiz onarım döngüsü, güvenli bir sonsuz savunmaya dönüşebilir. Onarım süresi, menzil, kapanış güvenliği ve saldırı temposu birlikte oynanarak dengelenir.

## Karar değişikliği biçimi

Yeni kayıt: tarih, ilgili kimlik, önceki karar, yeni karar, değişiklik nedeni, etkilenen aşamalar/dosyalar ve yeniden yapılacak kontrol. Teknik öneri başarılı deneyden sonra doğrulandı olarak işaretlenir; oyuncu tasarımı ayrıca kullanıcıyla netleştirilir.

## 2026-10-07 — Dış inceleme sonrası plan revizyonu

Kullanıcı mantıklı bulunan önerilerin belgelere işlenmesini istedi. Önceki M3 masaüstünde, mobil kontrol M5'te; M4 beş vagonlu tek aşamaydı. Yeni sıra T09–T11 ile erken cihaz ölçümü, mobil oynanış değerlendirmesi ve iki vagonlu ilk döngüyü içerir. `ROADMAP.md` yeniden kullanım tablosu, kapsam daraltma seçenekleri ve gerçek çalışma hızından sonra süre tahmini yaklaşımıyla güncellendi.

Eklenenler: kontrol/mermi, koşu devamı/metagelişim ve reklam/IAP ekonomi tasarımının erken konuşulması; riskle orantılı otomatik/elle test ayrımı. Korunanlar: NavMesh başlangıç tercihi, ortak hasar/yaşam döngüsü kuralları, pooling eklendiği anda temizlik kontrolü ve dronun oyun kimliğindeki önemi. Waypoint'e erken geçiş, gerekli yaşam döngüsü doğrulamalarını erteleme ve dronu otomatik yayın kapsamından çıkarma alınmadı.

Etkilenen belgeler: ROADMAP, DECISIONS, ARCHITECTURE, GAME_DESIGN ve geliştirme kaydı. Kod değişmedi; yeni kabul koşulları henüz denenmedi ve hiçbir geliştirme aşaması tamamlandı işaretlenmedi.

## 2026-10-08 — M6 teknik uygulama kaydı

StationSpawner + saf SpawnSchedule mevcut Run katmanında, EnemyProfile mevcut Enemies katmanında kuruldu. Yeni doğuşlar bütün vagonlardaki taşınan bedenleri sayar; dolu vagon istekleri kuyruk yerini serbest bırakıp kısa ertelenir. Geçersiz konum en fazla üç denemeden sonra iptal edilir; station bitişi bütün bekleyen üretimi kapatır. Havuz ısınması ve aktive etme ayrı kare bütçeleriyle yapılır. Bu teknik tercih ve başlangıç sayıları nihai denge kararı değildir. Normal/hızlı/dayanıklı test değerleri, 1/3/6+ programları ve açık Android kontrolü SPAWNING.md içinde. Kullanıcının mevcut iki taraflı/4–6 kapılı tren, camdan atış, iç zombi kalıcılığı ve rutin PC kontrolü kararları korunur.

## 2026-10-08 — Kullanıcı düzeltmesi: uçlar metal, daha dar cam, daha yavaş zombi

D24 güncellemesi: her vagonun iki yanındaki dört uç bölüm penceresiz, tamamen metal olur; yalnız dış kapılar arasında yan cam bulunur. Dış pencere çerçevesi 0,32 m, ara dikme 0,18 m; net açıklıklar MODEL_CONTRACT.md ve sahneden üretilen ölçü listesinde kayıtlıdır. Hareket/NavMesh sınırı ve kapı camı kuralları değişmez. Zombi hareket hızları normal 2,6 → 2,1, hızlı 3,8 → 3,0, dayanıklı 1,8 → 1,45 m/sn oldu; can, vuruş, saldırı temposu ve doğuş programı değişmedi. Hızlar Inspector'dan ayarlanabilir başlangıç dengesi olarak kalır.

## 2026-10-08 — D29: sabit turret kapsamı

Kullanıcı seçimi: normal ve gelişmiş turret için **vagon başına her türden 2** sınırı; şimdilik düşmanlar yalnız oyuncu/kapılara saldırır. Mermi bitiş patlaması yakındaki zombilere hasar verir. Dron kamikazesi ayrı M7c işidir. Teknik uygulama: mevcut ortak atış/metal-cam geometrisi, tek alan hasarı ve yakalanmış sahip yaşamıyla ödül; kurulum aktörleri yeniden kullanılır. Sayısal değerler ve PC kabulü [DEFENSES.md](DEFENSES.md) içinde. Turret ölüm/yeniden başlangıç ve vagon kalıcılığı aynı Run akışına bağlanır.

## 2026-10-08 — D30: gelir planının ertelenmesi ve sağlayıcı tercihi

Kullanıcı odağı önce oyun olarak seçti; IAP ekonomisi/ürün/fiyat henüz kararlaştırılmadı. İlk yayın Google Play, IAP teslimi onun faturalandırmasıyla hedeflenir. Reklam için Unity Ads tercih edilir; bu sağlayıcının gelir üstünlüğü doğrulanmış sayılmaz. Kullanıcı Battle 3×, hikâye devam/3× tekliflerini ve teklif reddinden sonra kısa sonuç geçiş reklamı fikrini yeniden belirtti. Önceki bağımsız reklam yerleşimi önerisi kullanıcı tarafından seçilmiş karar değildir; yeni tercih esas alınır. Gösterim sıklığı, reklam kaldırma hakkı, reklam yok/hata akışı ve güncel platform kuralları M9 somut servis tasarımında doğrulanır. Bu aşamada SDK eklenmedi.

## 2026-10-08 — D31: serbest turret kurulumu ve tren dizilimi

Kullanıcı sabit pad yaklaşımını kaldırdı: kendi vagonunda uygun zeminde satın alınan turret oyuncunun o andaki X/Z konumuna kurulur. Her türden 2/vagon kotası kalır. Dört yeniden kullanılabilir aktör/vagon; zombi/metal/başka turret üstüne kurulum ücret almadan reddedilir. Görünüm değişmeden gövde 0,22 × 0,6 × 0,22 m, carving 0,24 × 0,7 × 0,24 m; sahibi içinden yürür. Atama sonrası CharacterController yeniden etkinleştirildiğinde bu çarpışma hariç tutması yenilenir. Turret kurulmuş konum/mermiyle vagonda kalır.

D24 görünüm düzeltmesi: pencere üst kotu 1,9, üst metal 0,2 m; dikey dış çerçeve 0,42 ve ara dikme 0,32 m. Bütün görünür duvarlar 2,1 m üst kotta birleşir. Kapı üst çerçevesi gövdede sabittir, kapıyla gizlenmez. Cam/metal fiziği bu gerçek hacimlerle uyumludur; köşeler penceresiz kalır.

Tren +X yönüne gider; kuyruktan başa kapı/can dizilimi 6/10 → 5/20 → 5/26 → 4/32 → 4/40. En sağdaki güçlü vagonun sağında kapalı lokomotif/kontrol odası vardır; orası hedef, spawn alanı veya oynanabilir vagon değildir. Botlar henüz eklenmedi; M7c sonrası Mod/Bot sırası korunur.

Normal koşu her Restart'ta yeni rastgele tohum ve rastgele ilk vagon kullanır. Önceki 12345 sabit tohumu her oyunda aynı diziyi üretiyordu. Sabit tohum testlerde açık seçenek olarak korunur; fixture sonunda normal ayar geri yüklenir. Aynı vagonun tesadüfen tekrar seçilmesi mümkündür; sıra garantisi yoktur. ActualRunSeed ileride kayıtla RNG devamını çözmek için tek başına yeterli değildir; M8 mevcut RNG konumunu da korumalıdır.

Deneme kolaylığı için başlangıç parası RunShop.asset üzerinden geçici 1000 yapıldı; yayın dengesi kabulü değildir. Güncel model ölçüleri MODEL_CONTRACT ve üretilen CSV'lerdedir. Bu düzeltmelerden sonra sonraki mekanik M7c drondur.

## 2026-10-08 — D32: dron kamikazesi ve engeller

Kullanıcı onayı: mermisi biten dron erişebildiği en yakın zombiye gider; kapalı kapı/metalden geçmez, uygun hedef bulamazsa kısa süre arayıp kapanır. Patlama metalin arkasına hasar vermez. Süre ve değerler ayarlanır. İlk uygulama doğrudan küre taramasıyla açık yolu seçer; duvar üstünden geçmez, NavMesh veya karmaşık rota araması eklemez. Cam mermi geçirir ama fiziksel uçuşu engeller. Karanlıkta süre durur; kayıp hedef/atama süreyi sıfırlamaz. Oyuncuda bir dron, oyuncuyla kalıcılık, ortak atış/ödül ve yeniden kullanılan tek aktör. Ayrıntılar DRONE.md.

## 2026-10-08 — D33: Battle elemesi ve sonuç

Kullanıcı soruda onayladı: insan ölünce sırası gösterilip koşu biter; bütün botlar elenip yalnız insan kalınca birincilikle biter. Aynı karede elenenler aynı sırayı paylaşır. Teknik uygulama ölüm bayraklarını kare sonunda toplar; son iki katılımcı birlikte ölürse yaşayan kazanan ilan etmez, ortak sıra ve beraberlik gösterir. Ölü katılımcı aynı koşuda yeniden doğmaz. Yeni atama tüm yaşayan katılımcıları çakışmadan dağıtır; boş vagonlar yeni atamada seçilebilir. Yerel botlu Battle ile botsuz ilerleme modu ortak RunFlow kullanır. BOTS.md uygulama/sınır/ayar kaydıdır; gerçek multiplayer, sonuç reklamı veya kalıcı ödül eklenmedi.

## 2026-10-09 — D34: hareket yönü ve M8a kayıt temeli

Kullanıcı talebi: insan ve bot yürürken hareket yönüne baksın; geçerli ateş hedefi varsa yüzünü ona dönsün. Ortak motor bunu uygular; görsel model değişimi kuralı değiştirmez.

Önceden seçilen uygulama kapanınca mevcut koşuya devam / ölümde yeni koşu hedefi M8a yerel kayıtla uygulanır. Battle elemesi, vagonların ayrı durumu ve RNG ilerlemesi birlikte korunur. Mühendislik başlangıç ayarı beş saniyelik periyodik kayıt ve mobil yaşam döngüsü callback'leridir; Inspector'dan değişir. Editor test izolasyonu için otomatik devam varsayılan kapalı, F5/F6 açıktır. Bulut, kalıcı ödül/IAP ve kullanıcı menüsü bu adımda eklenmedi; Android süreç kapatma kabulü ayrı kalır. Sözleşme ve sınırlar PERSISTENCE.md içindedir.

## 2026-10-09 — D35: kalabalıkta vagon sınırı ve yumuşak yürüyüş dönüşü

Kullanıcı açık kapıda zombi baskısının oyuncuyu dışarı ittiğini ve WASD yürüyüş dönüşlerinin keskin olduğunu bildirdi. Ortak motor çarpışma sonrası gerçek kapsülü de MovementArea içinde tutar. İnsan ve bot, hedef yokken yürüyüş girdisi yönüne ayarlanabilir 720°/sn ile döner; nişan önceliği/hızlı dönüş korunur. Fiziksel itme boşta yön seçmez. Kapı/zombi navigasyonu ve model ölçüleri değişmez. Aynı kapı-kalabalık testi eski kodda başarısız, yeni kodda başarılıdır; uygulama ve kabul kaydı PLAYER_MOVEMENT.md içindedir. Bu bir M8a sonrası hata düzeltmesidir; sıradaki aşama M8b kalır.

## 2026-10-09 — D36: iki mod ve yayın doğrulama yolu

Kullanıcı ilk yayında Battle ve Solo'yu **eşit önemde** seçti; bir mod otomatik ertelenmez. Kullanıcı kamera/hasar/patlama hissi, cihaz akıcılığı ve ticari başarıyı önceliklendirdi; IAP ürünlerini kesinleştirmedi. Mühendislik önerisi: mevcut A54 ölçümü → hafif geri bildirim/akış → iki modda oynanış değerlendirmesi → tek vagon sanat örneği → bütçeli içerik/gelir/sınırlı yayın. Bu sıranın kabul koşulları RELEASE_PLAN.md içinde; kamera efektleri, telemetri, reklam/IAP ve yeni cihaz kabulü henüz uygulanmış değildir. Her telefonda sıfır düşüş veya garantili gelir/mağazada öne çıkarılma iddiası yapılmaz; cihaz bütçesi ve gerçek kullanıcı/gelir ölçümüyle ilerlenir.

Battle süre açıklaması: kullanıcı 5–8 dakika yanıtını hemen ardından kesinleştirmediğini belirtti; yaşayanların devam edebilmesini istedi. Zorunlu toplam yarış sayacı eklenmez; D33 sürer. 5–8 dakika yalnız ilk tempo değerlendirmesi için geçici referans, garanti veya bitiş kuralı değildir.

## 2026-10-09 — D37: Solo fiziksel ilerleme ve iki modda kalıcı güç

Kullanıcının güncel hedefi: Solo kuyruktaki en zayıf vagonda başlar; koşu parasıyla sıradaki daha güçlü vagon açılır, iç uç bağlantılarından yürünür. Açılmış vagonlar arasında geri geçiş serbest; kullanıcı soruda **yeni saldırılar bulunduğum vagona, mevcut iç zombiler korunsun** seçti. Solo'da rastgele atama kaldırılacak; Battle rastgele atama/eleme korunur. Yeni vagon bir defalık yardım/ganimet sağlayabilir; kesin loot listesi, iç bağlantı kapısı etkileşimi ve bedeller açık.

İki toplam para kaynağı hedefi: koşuda kazanılan/harcanan para ve sonuçtan sınırlı kazanılan/IAP ile satılabilen kalıcı para. Kalıcı silah açılışı sonrası koşu içi satın alma; küçük can/menzil/hız geliştirmeleri. Kullanıcı soruda **kalıcı güç iki modda da çalışsın** seçti; Battle güç normalizasyonu mühendislik önerisi seçilmedi. Ad/fiyat/üst sınır, ödül formülü ve paketler henüz seçilmedi. D30 Unity Ads/Google Play reklam akışı korunur. Solo ilk öğretici, sonra Battle açılışı hedef; koşul açık.

Güncel tasarım PROGRESSION_DESIGN.md, sıra RELEASE_PLAN.md. M8b mevcut cihaz temeli ardından M8s iki-vagon Solo, M8e minimum meta ve M8c kullanıcı akışı; M9a sanat örneği geçit ölçüsünden sonra öne alınabilir. Mevcut v1 kayıt ve kapalı uç geometrisi yeni Solo kabulü değildir; yeni sürüm/geçit ölçüsü gerekir. Bugünkü kod yeni ilerleme/kalıcı ekonomi veya yeni bot sunumunu henüz uygulamaz. Doğal bot davranışı/nickler kullanıcı hedefidir; gerçek online eşleşme iddiası yerine açık yerel yarış sunumu mühendislik önerisidir, kullanıcı tarafından kesinleşmiş metin değildir.

## 2026-10-09 — D38: yatay mobil ekran ve ölçüm kapsamı

Kullanıcı portre açılışın yanlış olduğunu ve joystick dokunuşlarının siyah kenarlarda `Move` izleri bıraktığını bildirdi. Yalnız iki yatay yön açık kalır. Güvenli alan dışındaki arka tampon her karede temizlenir; joystick görseli güvenli alanda tutulur, giriş başlangıcı yer değiştirmez. Kamera/framing, collider, atış ve hasar sözleşmeleri korunur. Mobilde açık 60 FPS hedefi ve seçilebilir 30 FPS API'si uygulanır; bu bütün cihazlarda 60 garantisi değildir.

Kullanıcı bu turda **kablolu sıcaklık/FPS testi** seçti. USB güç varken pil tüketimi sonucu üretilmez. Android Development APK'da yerel teşhis ve isteğe bağlı 10 dakika/60 havuz sınırında sentetik savaş fixture'ı bulunur; yayın build'inden çıkarılır. Normal koşu kaydı ayrı korunur. Ölçüm yöntemi ANDROID_PLAYTEST.md, gerçek A54 sonuçları A54_PERFORMANCE.md içinde. Bu test yeni Solo fiziksel ilerlemesi, son modeller/animasyonlar, üretim UI/reklamlar veya bütün cihazların yayın kabulü değildir. Açılıştaki AssetPackManager sınıf hatası çözülmeden temiz Android yayın kabulü verilmez.

## 2026-10-09 — D39: Solo manuel ara kapı ve takip

Kullanıcı seçti: satın alma ara kapının kilidini kaldırır; oyuncu yakında düğmeyle açar/kapatır. İç zombiler açık bağlantıdan takip eder, kapalıdan geçmez; ilk sürümde ara kapıya hasar vermez. Dış savunma kapısının dayanıklılık/onarım mekaniği bu kapıya uygulanmaz.

Uygulama: kuyrukta başlangıç, dört sıralı bağlantı, bağlı oda/koridor hareket hacimleri; eşikte güncel vagon bağlamı, tek Solo istasyon spawn bütçesi ve v2 yerel kayıt. Geçiş fiyatları 200/300/400/500 deneme değeridir; satın almak kapıyı otomatik açmaz. Geçitte canlı beden veya dron varken kapatma reddi mühendislik önlemidir. Dron 1,95 m uçuş yüksekliğini korur; üst metalin alt kotu 2,1 m'dir. Güncel ölçü CSV'leri MODEL_CONTRACT içindedir. Turretler eski vagonda kalır; yürüyüş ücretsiz koruma veya yeni koşu üretmez.

Kullanıcı A54 kısa geçiş testini **sonraya bıraktı**; PC kontrolü tamamlanınca cihaz kabulü beklemede kaydedilir. Yeni vagon ganimeti/meta/sonuç teslimi bu parçanın tamamlandığı iddia edilen özellikler değildir; sonraki işte içerik ve teslim kararı gerekir.

## 2026-10-09 — D40: Solo challenge ve küçük ilerleme parçaları

Kullanıcı açıkça belirtti: Solo ölümden sonra her yeni oyunda **en baştan**, en uzak istasyona ulaşma challenge'ı olarak başlar. Canlı koşuya ara verme kaydı ölüm sonrası devam değildir. Yeni koşuda ilk istasyon/kuyruk, kilitli vagonlar, yeni sandıklar ve başlangıç envanteri/altını; kalıcı cüzdan ve açılmış silahlar korunur. Reklamla canlandırma eski fikir olarak kalır; bu parçada eklenmez.

Kullanıcı testleri verimli tutmayı istedi: küçük parçalarda kritik birkaç kontrol, ardından iki parçanın birleşik testi; uzun eski kabul paketleri tekrar tekrar çalıştırılmaz. İlk küçük parçalar cephane sandığı ve ayrı kalıcı cüzdan/sonuç/silah açılışıdır. Fiyat/ödül sayıları mühendislik deneme değerleri ve Inspector ayarlarıdır; kalıcı güç, üretim ana menüsü, reklam/IAP SDK ve yeni cihaz kabulü bu parçaya dahil değildir. Sınırlar PROGRESSION_DESIGN.md içinde.

## 2026-10-09 — D41: ilk sınırlı güç geliştirmesi

Kullanıcı sıradaki aşamayı geliştirmeye yetki verdi; D37'deki iki mod kararı korunur. İlk küçük parça kalıcı can ve tüm oyuncu silahlarında menzildir. Mühendislik deneme değerleri: her biri 5 seviye, +%2/seviye (en fazla %10), fiyat 10/20/30/40/50 jeton. Sayısal değerler kullanıcı tarafından yayın dengesi olarak onaylanmış değildir; `RunProfile.upgrades` alanından küçük adımlar/fiyatlar ayarlanır. Satın alma sonuçta, uygulama yeni koşudadır; canlı kayıttan devam canı doldurmaz. Eski v1 profil para/silah/makbuz kaybetmeden göç eder. Hız, ana menü/öğretici, kamera/ses ve reklam/IAP bu parçaya dahil değildir. Sözleşme PROGRESSION_DESIGN.md içinde.

## 2026-10-09 — D42: üç vagonlu Hayatta Kalma pivotu

Kullanıcı D37/D39'daki manuel ara kapı ve yalnız güncel vagona yeni saldırı kararını değiştirdi. İlk somut iş ayrı sahnede üç vagon, başlangıçtan serbest geçiş, geniş sürekli açık iç bağlantılar ve üçünün iki tarafına saldırıdır. İçerideki zombiler oyuncuyu tren boyunca takip eder; boş eski vagonlara da yeni saldırılar sürer. Battle'ın ayrı vagon/yer değiştirme/boş vagon üretim politikası değişmez. Ürün adı Hayatta Kalma, teknik enum Solo kalır.

Gelecek koşu mağazası hedefleri: üç aşamalı kapı güçlendirme, tüm savunulan vagonların kapılarını onarma ve olası pencere yükseltmeleri; ölümde bu koşu yükseltmeleri sıfırlanır. Kalıcı profil mağazası ayrı kalır. Önce üç açık vagonlu oynanış değerlendirilir; ileride satın alarak vagon açma ve ganimet ayrıntılarını kullanıcı netleştirecek. Geçici 6/5/4 kapı, 3m geçit ve sınırlı spawn değerleri tasarım denemesi, nihai denge değildir. Ayrı nav/program/kayıt/ölçü dosyaları kullanılır; PC ve cihaz kabulü ayrı izlenir. Eski kabul raporları yeni pivot için kanıt sayılmaz.

## 2026-10-09 — D43: görünür Hayatta Kalma yolculuğu

Kullanıcı D42 sahnesinin kamera ve istasyon sunumunu netleştirdi. Kamera vagon merkezini değil oyuncunun sürekli X/Z konumunu izler. Yerde ödül/sandık şimdilik kaldırılır. İstasyon savunması zamanlı değildir: programın bütün gelecekteki ve kapasite bekleyen üretimleri tamamlanır; dışarıdaki/eşikteki son canlı bütünüyle trene bindiğinde kalkış başlar. Dışarıda öldürülenler de çözülmüş sayılır. İçeridekiler taşınır; yolculukta savaş, alışveriş ve kapı onarımı devam eder. Siyah ekran/sayaç/atama yoktur. İlk seyir 10 saniye, ayarlanabilir mühendislik denemesidir; hasarlı kapı yüzünden binemeyen son düşmanları gizli timeout ile silmeyiz.

İlk görsel seyrek yol atmosferi, ikinci görsel yalnız UI yerleşimi/sağ alt mini harita referansıdır. Bir ekranın bütün sanat tarzı kopyalanmaz. İki görsel istasyon kopyası yaklaşma/uzaklaşmayı görünür tutar. Yol direği/ekipman parçaları görsel köktedir; collider, gerçek ışık/gölge veya NavMesh sahibi değildir. Uzak sabit spawn ankrajları ve platform fiziği yalnız Survival'a aittir. Mini harita mevcut konumları çizer; ek dünya kamerası/RenderTexture gerektirmez. Battle ayrı sahnede aynı süre/karartma/atama politikasını korur. Ortak RunFlow'a isteğe bağlı boarding politikası eklenir, ikinci bir oyun motoru yazılmaz. Yeni policy content hash'i eski Survival koşusunu reddeder; kalıcı profil ve Battle kaydı korunur. Cihaz kabulü beklemededir.

## 2026-10-09 — D44: yumuşak başlangıç, okunur HUD ve koşu içi kapı güçlendirmesi

D42/D43 üzerindeki güncel kullanıcı değişiklikleri: Survival orta vagonda başlar. İlk istasyon yalnız orta vagona 4 zombi; ikinci 2/3/2 dağılımla toplam7; üçüncü3/4/3 toplam10. Sonra her bant +1/istasyon, bant başına12 üst sınır (toplam36); eşzamanlı24 toplam/8 vagon, havuz12/vagon korunur. Sayılar, ilk geliş/üretim aralıkları ve üst sınırlar bağımsız SurvivalPrograms Inspector verisidir. Intro profili kullanılır; Battle programlarına dokunulmaz. Bunlar yayın dengesi veya retention kanıtı değildir.

Survival dış kapı tabanları eşit20HP (deneme). Kullanıcı kapı hasarının onarımı sıfırlamamasını istedi: hem oyuncu hem kapı hasarı sırasında ilerleme korunur; uzaklaşma, hedef/can kimliği değişimi ve ölüm iptal eder. Door/actor damage bayrakları bağımsız ayarlanabilir kalır. Bu ortak varsayılan ve Battle sahnesindeki bağlar güncellenmiştir; Battle vagon dayanıklılık farkları korunur.

Kullanıcı güçlendirme kapsamını **tüm dış kapılar** olarak seçti; pencereler kırılmayan atış açıklıklarıdır. SurvivalShop ayrı katalog: tahta80altın, tel160altın; maksimumlar tabandan1.5x/2x (20→30→40). Sıra atlanamaz, tamamlanmış aşama tekrar alınamaz. Sağlam kapıya yeni kapasite eklenir, mevcut eksik can korunur; kırık kapı yükseltmeyle canlanmaz. Onarım ayrı üründür. Görsel tahta/tel collider/NavMesh/atış katmanı eklemez; her kapıda aşama başına tek paylaşılan mesh/renderer, kırılınca gizlenir. Yeni koşu tabanHP/seviye0; canlı kayıt seviye ve kapıHP tutarlılığını denetler. Yeni Survival içerik anahtarı eski koşuyu reddeder; kalıcı profil korunur.

Alt solda sıralı şarjör / yedek (sınırsızsa ---) / can / $ koşu altını; mini harita fon ve başlıksız. Survival mağazası veri güdümlü üç kaydırılabilir bölüm: solda silahlar, sağ üst sağlık/kapılar, sağ alt iki turret ve dron. Oyun alışverişte devam eder; Battle liste düzenini korur.1000 başlangıç altını önceki kullanıcının test talebidir, yayın dengesi değildir. Kalıcı hızlı onarım geliştirmesi ana menü/profil işine iki mod için açık görev olarak eklenir; bu aşamada uygulanmaz.

Performans: tekrar GUIStyle/LINQ kapı listesi üretimi Survival HUD'dan kaldırılır, metinler0.12s aralıkla güncellenir; shop ürün etiketleri/grupları önbelleğe alınır; kamera kadraj uzaklığı yalnız ekran/safe-area/FOV/hacim değiştiğinde hesaplanır. On-demand8s Editor örneği cihaz/ısı/pil kabulü değildir. Kısa kritik birleşik kontrol sonrası kullanıcı oyun dengesi gözlemiyle devam edilir.

D44 kısa kontrolünde, Restart'tan hemen savunmaya atlandığında önceki Flow'un tamamlanmış schedule'ının bir kare yeni kalkışı tetikleyebildiği görüldü. WaveCompleted artık observedFlow ile güncel run.Flow referansını da karşılaştırır; eski koşu bilgisi yeni dalganın bittiği sayılmaz. Test ayrıca başlangıçta schedule'ı temizler. Gerçek oyun ilerlemesini hızlandıran fixture ile normal yaklaşma akışı ayrıdır.

## 2026-10-09 — D45: Survival fiyatları, bomba atar ve mayın

Kullanıcı ilk can150, tahta600, normal turret500, gelişmiş900, SMG300, pompalı1000, rifle2500, bomba atar6000 istedi. Yanıtla tel1200 ve mayın250 seçildi. Başlangıç3000 test altını. Diğer ürün fiyatları ve Battle bot ekonomisi değiştirilmez. Mevcut Intro ödülü12 korunur. Tahta/tel tüm dış kapıların koşu gücüdür; pencere kırılma sistemi eklenmez.

Ortak WeaponDefinition/atış teslimi bomba atarı taşır; sınırlı6+12 mühimmat, temas patlaması90/2.4m, süpürülen uçuş. Ortak MineController yalnız düşmanla tetiklenir,80/2m; küçük collider-free gövde ve gerçek ışık kullanmayan kırmızı gösterge. Metal atış ve patlamayı durdurur; cam atış açıklığı kalır. Hasar/ödül mevcut Combat/KillRewards üzerinde; ayrı hasar veya para motoru yok. Sınırlı yeniden kullanılan havuzlar, canlı kayıt ve yeni koşuda temizleme birlikte eklendi. İlk sahne Survival; Battle sahnesine ve bot kullanımına dönünce iki mekanik de eklenecek açık görev.

Bomba atar yalnız koşu satın alımıyla denemeye açıktır; mevcut dört silahlı kalıcı profile yeni kilit eklenmez, ürün requiresWeaponUnlock=false. Daha sonra kalıcı açılış eklenirken profil göçü yapılacak. Zorluk bütçesi ve tempo önerisi docs/DIFFICULTY_AND_PRICES.md içindedir; mevcut4/7/10 başlangıcı değiştirilmez. Araştırılan Director tempo örneği, bizim tehdit maliyeti önerisinden ayrıdır.

## 2026-10-09 — D46: menzil, ucuz mühimmat ve kademeli tehdit

Kullanıcı pompalı/launcher menzilinin biraz azalmasını, yavaş bombanın hareketli hedefi daha iyi vurmasını, satın alınmış silahların daha fazla yedeğe sahip olmasını ve silah tükenince ucuz bir şarjör alabileceği düğmeyi istedi. Ayrıca tek tek istasyon sayısı yazmadan ayarlanabilir artan zorluk ve ardından Blender tren tasarımına geçiş istedi. Testler kısa kritik kontrollerle tutulur; A54 bu turda tekrar çalıştırılmaz.

İlk mühendislik ayarları: pompalı8→7m; launcher14→12m/hız14→24m/s; her satın alınan el silahı dolu+7 yedek şarjör. SMG50, pompalı125, rifle300, launcher750 altınla bir şarjör; bu fiyatlar kullanıcının oran örneğinden türetilmiş deneme değeridir. SurvivalShop tam fiyatlı tekrar silah satın alımını kapatır; Ammo ürünü mevcut Wallet/ShopService ve WeaponState üzerinden kapasiteye kadar bir yedek şarjör ekler. Boş silah normal süreyle dolmaya başlar, mevcut reload/tetik beklemesi atlanmaz. HUD düğmesi yalnız seçili silahın şarjör+yedeği tamamen boşken görünür; mağazadan daha önce de yedek alınabilir. Başlangıç10000 test altını korunur.

AutoAim seçili hedef konumundan hız örnekler; bomba atar sabit hızlı uçuş için ilerideki kesişim noktasını hedefler. Sahne araması/homing eklenmez. Metal tahmin hattını kapatıyorsa doğrudan nişan; süpürülen mermi ve metal korumalı patlama aynı Combat sistemindedir. Ortak silah assetleri nedeniyle menzil/stoğun Battle oyuncusuna etkisi vardır; Battle katalog ve bot alışveriş kuralı bu adımda değiştirilmez. Ucuz şarjör Battle entegrasyonuyla ayrıca bağlanacaktır.

Survival ilk4/7/10 dalgasından sonra bir StationDefinition tehdit programı kullanır: istasyon4 bütçe13, +3/istasyon, en fazla160. Temel/hızlı/dayanıklı maliyet1/2/4, ağırlık6/2/1, açılma4/5/8; tür dalga sınırı120/40/20. Tek bütçe üç vagona bölünür. İlk geliş4s, akış aralığı2.5s, vagon kaydırma0.35s; canlı24/8, kuyruk32, havuz12/vagon korunur. Inspector verisi deneme değeridir, keyif veya yayın dengesi kanıtı değildir. Plan istasyon+içerikten deterministik yeniden üretilir; koşu tohumuyla karışım farklılaştırma bu sürümde yoktur. Battle'ın eski bant sistemi isteğe bağlı bütçe kapalı olduğu için korunur. Kayıt içerik anahtarı yeni parametreleri taşır; eski içerikli aktif koşu reddedilebilir, kalıcı profil korunur.

PC38 silah+45 Run+13 ekonomi testi ve kısa canlı mühimmat/mayın/bomba/kayıt kontrolü PASS; mağaza kartları gözle kontrol edildi, Console0 hata. Kanıt generated/survival-ammo-aim-budget-pc-acceptance.txt. Sonraki sanat işi MODEL_CONTRACT ölçülerine göre Blender giydirmedir; geometri boyutları/collider/nav bu adımda değiştirilmedi. Kalıcı hızlı onarım, launcher kalıcı kilidi, yeni Android yük ve nihai ekonomi dengesi açık kalır.
