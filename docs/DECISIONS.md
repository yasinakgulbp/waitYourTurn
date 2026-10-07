# Karar kaydı

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

## Teknik öneriler

| Kimlik | Öneri | Doğrulama aşaması |
| --- | --- | --- |
| T01 | İç/dış NavMesh + kapı başına kontrollü NavMeshLink | M1: kapalıyken alternatif geçiş yok, açıkken gerçek geçiş var. |
| T02 | Kapı/zombi/oyuncu için ortak hasar ve sağlık sözleşmesi | M2: sağlık sınırları, tek ölüm/ödül, farklı kaynaklar. |
| T03 | İlk silahlar hitscan/pellet; satın alınmış projectile assetleri VFX | M5: isabet, görüş/engel ve görsel uyumu. Yavaş projectile gelecekte ayrı resolver kullanır. |
| T04 | Küçük enemy durum makinesi, veri tabanlı tür farklılıkları | M3/M6: türler aynı döngüyü kullanır; gereksiz framework eklenmez. |
| T05 | Spawn canlı sınırları, havuzlar ve dağıtılmış AI/path güncellemeleri | M6/M9: cihazda ölçüm; sayısal bütçeler profil sonucuyla belirlenir. |
| T06 | Sabit savunma slotları; ilk mağazada otomatik para ödülü | M7: daha sade mobil etkileşim. Fiziksel para toplama ileride eklenebilir. |
| T07 | Rastgele atamada eşit vagon olasılığı; aynı vagon tekrar çıkabilir | M4: tasarım denemesi. Kullanıcı henüz bu dağılımı seçmedi. |
| T08 | Ölümde oyun biter; kararma/onarım/satın alma eski koşuyu yeniden başlatamaz | M2/M4: ölüm her evrede geçerli. |

## Uygulama öncesi veya ilgili aşamada netleştirilecekler

Bu liste ilk navigasyon/hasar denemesini engellemez. İlgili özellik başlamadan karar verilir; yanıt alınmamış tercih kesinleşmiş gibi uygulanmaz.

| Konu | İlk öneri / açık detay | Gerektiği aşama |
| --- | --- | --- |
| Hedef Android cihazları | En az bir düşük ve bir orta sınıf gerçek cihaz seçmek; ölçüm olmadan sabit FPS garantisi yok. | M1 ölçüm düzeni, M6 ilk kalabalık bütçesi |
| Vagonlar arası geçiş | İlk kapsamda oyuncu atandığı vagonda; bağlantılar varsa açıkça tanımlanır. Zombilerin başka vagona gidip gitmeyeceği ayrı. | M3/M4 |
| Onarımda kapanış ve saldırı | Geçitte karakter varsa kapanış beklesin; ateş edilebilir. Hasar alınca onarımın iptali henüz seçilmedi. | M3 |
| Doğuş güvenliği | Duvar/karakter içine doğmama + kısa doğuş koruması önerisi; içerideki zombilerin konumu korunur. | M4 |
| Kalkış uyarısı | İlk öneri 60 saniye savunma sonrasında 3 saniye uyarı; fade/yavaşlama süreleri ayrı ayarlar. | M4 |
| Düşman hedef önceliği | Atanmış vagonda oyuncu, turret veya başka hedeflerin önceliği; boş vagondaki zombi davranışı. | M3/M7 |
| Savunmaların canı ve patlaması | Zombiler turret/drona zarar verebilir mi? Turretin mermi bitiş patlaması sadece efekt mi? Dron patlaması duvar/kapıya nasıl etki eder? | M7 |
| Turret sınırının kapsamı | Her türden 2 adet örneği: oyuncu başına mı vagon başına mı? Öneri vagon/slot başına, koşu toplamı ayrıca sınırlı. | M7 |
| Silah mermisi ve reload | Turret/dron sınırı belli; oyuncu silahlarının şarjör, yedek mermi, reload ve satın alma davranışı henüz kesin değil. | M5 |
| Mağaza erişimi | Savaşta açık mı, yalnızca yolculukta mı? Öneri gerçek zamanı durdurmayan kolay mobil UI. | M7 |
| Para toplama | Kullanıcı para toplama istedi; ilk öneri ödülü otomatik cüzdana yazmak. Görsel pickup/yerden toplama tercihi açık. | M7 |
| Koşu devamı / metagelişim | Uygulama kapanınca koşuya devam ve koşular arası satın alımlar ayrı tasarlanacak. | M8 |

## Bilinen tasarım riskleri

- Kalıcı kapı hasarı ve rastgele vagon ataması, oyuncuyu çok tehlikeli bir vagona getirebilir. Bu istenen risk/şans hissini destekler; anında kaçınılmaz ölüm olmaması için doğuş güvenliği gerekir.
- Turretler eski vagonlarda kaldığından oyuncu dışında öldürme/ödül devam edebilir. Karanlıkta tüm oynanış durur; görünür zamanda bu etki ekonomi dengesiyle değerlendirilir.
- Süreli kalkışta içeride kalan zombi sayısı her istasyonda artabilir. Global canlı sınırı sadece yeni spawnı değil taşınan zombileri de saymalıdır; içeri girmiş zombi sırf bütçe doldu diye yok edilmez.
- Kapıyı onarıp tekrar kırdırmanın ücretsiz onarım döngüsü, güvenli bir sonsuz savunmaya dönüşebilir. Onarım süresi, menzil, kapanış güvenliği ve saldırı temposu birlikte oynanarak dengelenir.

## Karar değişikliği biçimi

Yeni kayıt: tarih, ilgili kimlik, önceki karar, yeni karar, değişiklik nedeni, etkilenen aşamalar/dosyalar ve yeniden yapılacak kontrol. Teknik öneri başarılı deneyden sonra doğrulandı olarak işaretlenir; oyuncu tasarımı ayrıca kullanıcıyla netleştirilir.
