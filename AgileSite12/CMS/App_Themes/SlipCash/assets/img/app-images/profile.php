<?php 
if (false == profile() ) {
    return;
}
?>
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Slip.Cash Signin</title>

    <link rel="stylesheet" type='text/css' href="./assets/style/slip.cash.css">
</head>

<body>
    <div class="profile-container">
        <div class="brand">
            <img src="./assets/images/logo.png" alt="Slip.Cash">
        </div>

        <div class="profile">
            <div class="profile__header">
                <div class="header__wrapper">
                    <img src="<?=!empty($_SESSION['profile']['imageid']) ? $_SESSION['profile']['imageid'] : './uploads/avatars/blank.jpg'?>"
                        alt="">
                    <div class="username">
                        <h2><?=$_SESSION['profile']['firstname']?> <?=$_SESSION['profile']['lastname']?></h2>
                    </div>

                    <div class="profile-quote">
                        <span class='quote'><?=nl2br($_SESSION['profile']['custommessage'])?></span>
                    </div>
                    <a onclick="togglemodal('modal-share')"
                        class='profile-link'>https://slip.cash/$<?=strtolower($_SESSION['profile']['username'])?></a>
                </div>
                <!--
                <nav class="menu">
                        <div>Links</div>
                        <div>Social</div>
                        <div>Affilates</div>
                        <div>Scan</div>
                    </nav>-->
            </div>
            <div class="profile__body">
                <div class="body__wrapper">
                    <div class="payment-grid">

                        <?php 
                        $links = [
                            'paypallink'      => ['endpoint' => 'https://www.paypal.me/',          'background' => '/images/Paypal.png'],
                            'venmolink'       => ['endpoint' => 'https://venmo.com/code?user_id=', 'background' => '/images/Venmo.png'],
                            'cashapplink'     => ['endpoint' => 'https://cash.app/',               'background' => '/images/Cash-app.png'],
                            'applepaylink'    => ['endpoint' => 'sms://',                          'background' => '/images/apple-cash.png'],
                            'facebookmsglink' => ['endpoint' => 'sms://',                          'background' => '/images/messengerbutton.png'],
                            'facebookpaymentslink' => ['endpoint' => 'sms://',                     'background' => '/images/on.png'],
                            'instagramlink'  => ['endpoint' => 'https://instagram.com/',           'background' => '/images/.png'],
                            'googlepaylink'  => ['endpoint' => 'upi://pay?pa=',                    'background' => '/images/.png'],
                            'alipaylink'     => ['endpoint' => '', 'background' => '/images/alipay.png'],  
                            'wechatlink'     => ['endpoint' => '', 'background' => '/images/wechat.png'],  
                            'coinbase'       => ['endpoint' => '', 'background' => '/images/coinbase.png'],  
                            'crypto'         => ['endpoint' => '', 'background' => '/images/cyrpto.png'],  
                            'mobie'          => ['endpoint' => '', 'background' => '/images/mobie.png'],  
                            'cryptocom'      => ['endpoint' => '', 'background' => '/images/cryptocom.png'],  
                            'websitelink'    => ['endpoint' => '', 'background' => ''],  
//                            'websitetextlink' => ['endpoint' => '', 'background' => ''],  
                        ];  
                        foreach($links as $link => $details):
                            if(empty($_SESSION['profile'][$link]))
                                continue; //skip empty link
                        ?>
                        <div class="paylink">
                            <a href="<?=$details['endpoint']?><?=$_SESSION['profile'][$link] ?>" target='_blank'>
                                <div class="icon">
                                    <img src="<?=$details['background']?>">
                                </div>
                            </a>
                        </div>
                        <?php endforeach;?>

                    </div>
                    <?php if( !empty($_SESSION['user']) && $_SESSION['user']['id'] == $_SESSION['profile']['id'] ):?>
                    <div class="actions">
                        <button class='button' onclick="togglemodal('modal-payments')">Edit Payment Profiles</button>
                        <a class='button button-inverted' href="./destroy.php">Sign Out</a>
                    </div>

                    <?php endif; ?>
                </div>

            </div>
        </div>
    </div>



    <?php if( !empty( $_SESSION['user'] ) ): 
        if( $_SESSION['user']['id'] != $_SESSION['profile']['id'] ):?>
        
        <a class='my-profile' href="/$<?=$_SESSION['user']['username']?>">
            <ion-icon name="share-alt"></ion-icon>
        </a>
                    
    <?php endif; endif; ?>


    <div class="modal modal-share" style='display:none'>
        <div class="modal-wrapper">
            <div class="modal-header">

            </div>
            <div class="modal-body">
                <div class="brand">
                    <img src="./assets/images/logo.png" alt="Slip.Cash">
                </div>
                <h4>Share This Link</h4>
                <a class='profile-link'
                    href='https://slip.cash/$<?=strtolower($_SESSION['profile']['username'])?>'>https://slip.cash/$<?=strtolower($_SESSION['profile']['username'])?></a>
                <img width="250px" src="https://slip.cash/<?=$_SESSION['profile']['qr_code_url']?>" alt="" srcset="">
                <div class="inline-buttons">
                    <a href="#" class='button'>Share</a>
                    <a onclick="togglemodal('modal-share')" class='button'>Close</a>
                </div>
            </div>
        </div>
    </div>

    <div class="modal modal-payments" style='display:none'>
        <div class="modal-wrapper">
            <div class="modal-header"></div>
            <div class="modal-body">

                <form class='form' action="./process.php?action=updatepayments" method='post'>
                    <h3>Update Payment Profiles</h3>
                    <?php foreach($links as $link => $details):?>
                    <div class="input-form-group">
                        <label><img src="./assets/images/buttonbgs/<?=$details['background']?>"></label>
                        <input name='<?=$link?>' type="text" placeholder='' value="<?=$_SESSION['user'][$link] ?>">
                    </div>
                    <?php endforeach;?>
                    <div class="actions">
                        <button class='button button-inverted' type='button'
                            onclick="togglemodal('modal-payments')">Close</button>
                        <input class='button ' type="submit" value="Save Changes">
                    </div>
            </div>
        </div>




        <script src="https://unpkg.com/ionicons@4.5.10-0/dist/ionicons.js"></script>

        <script>
            x = function (tag) {
                elements = document.querySelectorAll(tag)
                return (elements.length > 1) ? elements : elements[0]
            }
            togglemodal = function (modal) {

                if (x('.' + modal).style.display == 'none') {
                    document.getElementsByClassName('profile-container')[0].classList.add('blur');
                    x('.' + modal).style.display = 'block';

                    return;
                } else {
                    console.log('hide modal')
                    document.getElementsByClassName('profile-container')[0].classList.remove('blur')

                    x('.' + modal).style.display = 'none';

                }

            }

            init();
        </script>


</body>

</html>